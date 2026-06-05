using LastBiteNew.Models;
using LastBiteNew.Services;
using LastBiteNew.ViewModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Restaurant.Controllers
{
    [Area("Restaurant")]
    [Authorize(Roles = "RestaurantOwner")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Models.Restaurant?> GetOwnerRestaurantAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == userId);
        }

        // GET: /Restaurant/Reservations/Index
        public async Task<IActionResult> Index(string? status, int? packageId, string? range)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var packageIds = await _context.FoodPackages
                .Where(p => p.RestaurantId == restaurant.RestaurantId)
                .Select(p => p.PackageId)
                .ToListAsync();

            var rows = await FilteredQuery(packageIds, status, packageId, range)
                .Select(r => new ReservationRowViewModel
                {
                    ReservationId = r.ReservationId,
                    ReservationCode = r.ReservationCode,
                    CustomerName = r.Customer.FullName,
                    CustomerEmail = r.Customer.Email ?? string.Empty,
                    PackageName = r.FoodPackage.Name,
                    Quantity = r.Quantity,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    ReservedAt = r.ReservedAt,
                    PickupStartTime = r.FoodPackage.PickupStartTime,
                    PickupEndTime = r.FoodPackage.PickupEndTime
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentRange = range;
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.PackageId = packageId;
            ViewBag.PackageName = packageId.HasValue
                ? await _context.FoodPackages.Where(p => p.PackageId == packageId.Value).Select(p => p.Name).FirstOrDefaultAsync()
                : null;
            return View(rows);
        }

        // GET: /Restaurant/Reservations/Export  (CSV — P-16)
        public async Task<IActionResult> Export(string? status, int? packageId, string? range)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var packageIds = await _context.FoodPackages
                .Where(p => p.RestaurantId == restaurant.RestaurantId)
                .Select(p => p.PackageId)
                .ToListAsync();

            var rows = await FilteredQuery(packageIds, status, packageId, range)
                .Select(r => new
                {
                    r.ReservationCode,
                    Customer = r.Customer.FullName,
                    Email = r.Customer.Email,
                    Package = r.FoodPackage.Name,
                    r.Quantity,
                    r.TotalPrice,
                    r.Status,
                    r.ReservedAt,
                    Pickup = r.FoodPackage.PickupStartTime
                })
                .ToListAsync();

            string Esc(string? s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Code,Customer,Email,Package,Quantity,Total (JD),Status,Reserved At,Pickup");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Esc(r.ReservationCode), Esc(r.Customer), Esc(r.Email), Esc(r.Package),
                    r.Quantity, r.TotalPrice.ToString("0.00"), r.Status,
                    r.ReservedAt.ToString("yyyy-MM-dd HH:mm"), r.Pickup.ToString("yyyy-MM-dd HH:mm")));
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"reservations-{DateTime.Now:yyyyMMdd-HHmm}.csv");
        }

        private IQueryable<Reservation> FilteredQuery(List<int> packageIds, string? status, int? packageId, string? range)
        {
            var q = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.FoodPackage)
                .Where(r => packageIds.Contains(r.PackageId));

            if (packageId.HasValue)
                q = q.Where(r => r.PackageId == packageId.Value);

            if (Enum.TryParse<ReservationStatus>(status, out var st))
                q = q.Where(r => r.Status == st);

            var nowUtc = DateTime.UtcNow;
            if (range == "today")
                q = q.Where(r => r.ReservedAt >= nowUtc.Date);
            else if (range == "week")
                q = q.Where(r => r.ReservedAt >= nowUtc.AddDays(-7));

            return q.OrderByDescending(r => r.ReservedAt);
        }

        // POST: /Restaurant/Reservations/Complete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage)
                .FirstOrDefaultAsync(r => r.ReservationId == id
                    && r.FoodPackage.RestaurantId == restaurant.RestaurantId);

            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["Error"] = "Only pending reservations can be marked as completed.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Status = ReservationStatus.Completed;
            reservation.CompletedAt = DateTime.UtcNow;

            NotificationHelper.Add(_context, reservation.CustomerId, "Pickup confirmed ✓",
                $"Your pickup for \"{reservation.FoodPackage.Name}\" is confirmed. You can now leave a review!",
                NotificationType.ReviewReminder, reservation.ReservationId);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation {reservation.ReservationCode} marked as completed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Restaurant/Reservations/NoShow/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> NoShow(int id)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage)
                .FirstOrDefaultAsync(r => r.ReservationId == id
                    && r.FoodPackage.RestaurantId == restaurant.RestaurantId);

            if (reservation == null) return NotFound();

            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["Error"] = "Only pending reservations can be marked as no-show.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Status = ReservationStatus.NoShow;

            // Return quantity to the package
            reservation.FoodPackage.RemainingQuantity =
                Math.Min(reservation.FoodPackage.TotalQuantity,
                         reservation.FoodPackage.RemainingQuantity + reservation.Quantity);

            if (reservation.FoodPackage.Status == PackageStatus.SoldOut)
                reservation.FoodPackage.Status = PackageStatus.Active;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation {reservation.ReservationCode} marked as no-show. Quantity returned to package.";
            return RedirectToAction(nameof(Index));
        }
    }
}
