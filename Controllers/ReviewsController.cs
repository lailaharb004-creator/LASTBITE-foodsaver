using LastBiteNew.Models;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    [Route("Reviews")]
    [Authorize(Roles = "Customer")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Reviews/Create/5  (5 = reservationId)
        [HttpGet("Create/{reservationId:int}")]
        public async Task<IActionResult> Create(int reservationId)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage).ThenInclude(p => p.Restaurant)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.CustomerId == userId);

            if (reservation == null) return NotFound();

            var error = await EligibilityErrorAsync(reservation);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("MyReservations", "Reservations");
            }

            return View(new ReviewViewModel
            {
                ReservationId = reservation.ReservationId,
                RestaurantName = reservation.FoodPackage.Restaurant.Name,
                PackageName = reservation.FoodPackage.Name
            });
        }

        // POST: /Reviews/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewViewModel vm)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.FoodPackage).ThenInclude(p => p.Restaurant)
                .FirstOrDefaultAsync(r => r.ReservationId == vm.ReservationId && r.CustomerId == userId);

            if (reservation == null) return NotFound();

            var error = await EligibilityErrorAsync(reservation);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("MyReservations", "Reservations");
            }

            if (!ModelState.IsValid)
            {
                vm.RestaurantName = reservation.FoodPackage.Restaurant.Name;
                vm.PackageName = reservation.FoodPackage.Name;
                return View(vm);
            }

            var restaurantId = reservation.FoodPackage.RestaurantId;

            _context.Reviews.Add(new Review
            {
                ReservationId = reservation.ReservationId,
                CustomerId = userId!,
                RestaurantId = restaurantId,
                Rating = vm.Rating,
                Comment = string.IsNullOrWhiteSpace(vm.Comment) ? null : vm.Comment.Trim(),
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // BR-015: only visible reviews count toward the rating.
            var visible = _context.Reviews.Where(r => r.RestaurantId == restaurantId && r.IsVisible);
            var count = await visible.CountAsync();
            var avg = count > 0 ? await visible.AverageAsync(r => (double)r.Rating) : 0;

            var restaurant = reservation.FoodPackage.Restaurant;
            restaurant.AverageRating = (decimal)Math.Round(avg, 2);
            restaurant.TotalReviews = count;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you! Your review has been submitted.";
            return RedirectToAction("MyReservations", "Reservations");
        }

        // Eligibility per FR-09 / BR-013 / BR-014. Returns an error message, or null if eligible.
        private async Task<string?> EligibilityErrorAsync(Reservation r)
        {
            if (r.Status != ReservationStatus.Completed)
                return "You can only review completed pickups.";

            if (r.CompletedAt == null || DateTime.UtcNow > r.CompletedAt.Value.AddHours(48))
                return "The 48-hour review window for this reservation has closed.";

            var alreadyReviewed = await _context.Reviews.AnyAsync(x => x.ReservationId == r.ReservationId);
            if (alreadyReviewed)
                return "You have already reviewed this reservation.";

            return null;
        }
    }
}
