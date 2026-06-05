using LastBiteNew.Models;
using LastBiteNew.Services;
using LastBiteNew.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
            => _context = context;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Restaurants
                .AsNoTracking()
                .Include(r => r.Owner)
                .AsQueryable();

            if (Enum.TryParse<RestaurantStatus>(status, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var restaurants = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RestaurantRowViewModel
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    OwnerName = r.Owner.FullName,
                    OwnerEmail = r.Owner.Email ?? "",
                    Category = r.Category,
                    City = r.City,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    TotalPackages = r.FoodPackages.Count,
                    AverageRating = r.AverageRating
                }).ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(restaurants);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            restaurant.Status = RestaurantStatus.Approved;
            restaurant.ApprovedAt = DateTime.UtcNow;
            restaurant.ApprovedByAdminId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            NotificationHelper.Add(_context, restaurant.OwnerId, "Restaurant approved 🎉",
                $"Your restaurant \"{restaurant.Name}\" has been approved. You can now publish food packages.",
                NotificationType.RestaurantApproval, restaurant.RestaurantId);

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{restaurant.Name}' has been approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            restaurant.Status = RestaurantStatus.Rejected;
            restaurant.RejectionReason = reason;

            NotificationHelper.Add(_context, restaurant.OwnerId, "Application rejected",
                $"Your restaurant \"{restaurant.Name}\" was not approved." +
                (string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason}"),
                NotificationType.RestaurantApproval, restaurant.RestaurantId);

            await _context.SaveChangesAsync();

            TempData["Error"] = $"'{restaurant.Name}' has been rejected.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.FoodPackages)
                .FirstOrDefaultAsync(r => r.RestaurantId == id);
            if (restaurant == null) return NotFound();

            restaurant.Status = RestaurantStatus.Suspended;
            foreach (var pkg in restaurant.FoodPackages
                .Where(p => p.Status == PackageStatus.Active))
                pkg.Status = PackageStatus.Cancelled;

            await _context.SaveChangesAsync();
            TempData["Error"] = $"'{restaurant.Name}' has been suspended.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsuspend(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            restaurant.Status = RestaurantStatus.Approved;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{restaurant.Name}' has been restored.";
            return RedirectToAction(nameof(Index));
        }
    }
}