using LastBiteNew.Models;
using LastBiteNew.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context) => _context = context;

        // GET: /Admin/Reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AdminReviewRowViewModel
                {
                    ReviewId = r.ReviewId,
                    RestaurantName = r.Restaurant.Name,
                    CustomerName = r.Customer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    IsVisible = r.IsVisible,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(reviews);
        }

        // POST: /Admin/Reviews/ToggleVisibility/5  (BR-015)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsVisible = !review.IsVisible;
            await _context.SaveChangesAsync();

            // Recalculate the restaurant's rating from visible reviews only.
            var visible = _context.Reviews.Where(r => r.RestaurantId == review.RestaurantId && r.IsVisible);
            var count = await visible.CountAsync();
            var avg = count > 0 ? await visible.AverageAsync(r => (double)r.Rating) : 0;

            var restaurant = await _context.Restaurants.FindAsync(review.RestaurantId);
            if (restaurant != null)
            {
                restaurant.AverageRating = (decimal)System.Math.Round(avg, 2);
                restaurant.TotalReviews = count;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = review.IsVisible ? "Review is now visible." : "Review hidden.";
            return RedirectToAction(nameof(Index));
        }
    }
}
