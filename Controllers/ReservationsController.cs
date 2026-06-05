using LastBiteNew.Models;
using LastBiteNew.Services;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    [Route("Reservations")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Reservations/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int packageId, int quantity)
        {
            var userId = _userManager.GetUserId(User)!;

            var package = await _context.FoodPackages
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.PackageId == packageId
                                       && p.Status == PackageStatus.Active
                                       && p.RemainingQuantity > 0
                                       && p.PickupEndTime > DateTime.Now
                                       && p.Restaurant.Status == RestaurantStatus.Approved);

            if (package == null)
            {
                TempData["Error"] = "This package is no longer available.";
                return RedirectToAction("Index");
            }

            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.PackageId == packageId
                            && r.CustomerId == userId
                            && r.Status != ReservationStatus.CancelledByCustomer
                            && r.Status != ReservationStatus.CancelledByRestaurant);

            if (alreadyReserved)
            {
                TempData["Error"] = "You have already reserved this package.";
                return RedirectToAction("Details", "Packages", new { id = packageId });
            }

            quantity = Math.Clamp(quantity, 1, package.RemainingQuantity);

            var code = await GenerateUniqueCodeAsync();
            var total = package.DiscountedPrice * quantity;

            var reservation = new Reservation
            {
                PackageId = packageId,
                CustomerId = userId,
                Quantity = quantity,
                TotalPrice = total,
                CommissionAmount = Math.Round(total * 0.10m, 2),
                Status = ReservationStatus.Pending,
                ReservationCode = code,
                ReservedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);

            package.RemainingQuantity -= quantity;
            if (package.RemainingQuantity == 0)
                package.Status = PackageStatus.SoldOut;

            NotificationHelper.Add(_context, package.Restaurant.OwnerId, "New reservation",
                $"{quantity} × \"{package.Name}\" reserved (code {code}).",
                NotificationType.Reservation, package.PackageId);

            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { id = reservation.ReservationId });
        }

        // GET: /Reservations/Confirmation/5
        [HttpGet("Confirmation/{id:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage)
                    .ThenInclude(p => p.Restaurant)
                .FirstOrDefaultAsync(r => r.ReservationId == id && r.CustomerId == userId);

            if (reservation == null) return NotFound();

            var vm = new ReservationConfirmViewModel
            {
                ReservationId = reservation.ReservationId,
                ReservationCode = reservation.ReservationCode,
                PackageName = reservation.FoodPackage.Name,
                RestaurantName = reservation.FoodPackage.Restaurant.Name,
                RestaurantAddress = reservation.FoodPackage.Restaurant.Address,
                Quantity = reservation.Quantity,
                TotalPrice = reservation.TotalPrice,
                PickupStartTime = reservation.FoodPackage.PickupStartTime,
                PickupEndTime = reservation.FoodPackage.PickupEndTime
            };

            return View(vm);
        }

        // GET: /Reservations/MyReservations
        [HttpGet("MyReservations")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyReservations(string? status)
        {
            var userId = _userManager.GetUserId(User);

            // P-09: show active (Pending) reservations + recent activity (last 7 days).
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var query = _context.Reservations
                .Include(r => r.FoodPackage)
                    .ThenInclude(p => p.Restaurant)
                .Where(r => r.CustomerId == userId
                         && (r.Status == ReservationStatus.Pending || r.ReservedAt >= sevenDaysAgo));

            if (Enum.TryParse<ReservationStatus>(status, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var rows = await query
                .OrderByDescending(r => r.ReservedAt)
                .Select(r => new MyReservationRowViewModel
                {
                    ReservationId = r.ReservationId,
                    ReservationCode = r.ReservationCode,
                    PackageName = r.FoodPackage.Name,
                    RestaurantName = r.FoodPackage.Restaurant.Name,
                    City = r.FoodPackage.Restaurant.City,
                    Quantity = r.Quantity,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    ReservedAt = r.ReservedAt,
                    PickupStartTime = r.FoodPackage.PickupStartTime,
                    PickupEndTime = r.FoodPackage.PickupEndTime,
                    CompletedAt = r.CompletedAt,
                    HasReview = _context.Reviews.Any(rev => rev.ReservationId == r.ReservationId)
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(rows);
        }

        // POST: /Reservations/Cancel/5
        [HttpPost("Cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage).ThenInclude(p => p.Restaurant)
                .FirstOrDefaultAsync(r => r.ReservationId == id && r.CustomerId == userId);

            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["Error"] = "Only pending reservations can be cancelled.";
                return RedirectToAction("MyReservations");
            }

            // BR-011: cancellation allowed only up to 1 hour before pickup starts.
            if (DateTime.UtcNow >= reservation.FoodPackage.PickupStartTime.AddHours(-1))
            {
                TempData["Error"] = "Cancellations are only allowed up to 1 hour before the pickup window starts.";
                return RedirectToAction("MyReservations");
            }

            reservation.Status = ReservationStatus.CancelledByCustomer;
            reservation.CancellationReason = string.IsNullOrWhiteSpace(reason)
                ? null
                : (reason.Length > 300 ? reason.Trim()[..300] : reason.Trim());

            // Return the quantity to the pool if the package can still be sold.
            var package = reservation.FoodPackage;
            if ((package.Status == PackageStatus.Active || package.Status == PackageStatus.SoldOut)
                && package.PickupEndTime > DateTime.Now)
            {
                package.RemainingQuantity += reservation.Quantity;
                if (package.Status == PackageStatus.SoldOut)
                    package.Status = PackageStatus.Active;
            }

            NotificationHelper.Add(_context, reservation.FoodPackage.Restaurant.OwnerId,
                "Reservation cancelled",
                $"Reservation {reservation.ReservationCode} for \"{reservation.FoodPackage.Name}\" was cancelled by the customer.",
                NotificationType.Reservation, reservation.PackageId);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation {reservation.ReservationCode} was cancelled.";
            return RedirectToAction("MyReservations");
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string code;
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            do
            {
                var bytes = new byte[8];
                rng.GetBytes(bytes);
                code = "LB-" + new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
            }
            while (await _context.Reservations.AnyAsync(r => r.ReservationCode == code));
            return code;
        }
    }
}
