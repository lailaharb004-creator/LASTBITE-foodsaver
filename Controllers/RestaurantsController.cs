using LastBiteNew.Models;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    [Route("Restaurants")]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RestaurantsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Restaurants/5  — public profile (FR-10 / P-04)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Profile(int id)
        {
            // Only approved restaurants have public profiles.
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.RestaurantId == id && r.Status == RestaurantStatus.Approved);

            if (restaurant == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var reservedIds = userId != null
                ? await _context.Reservations
                    .Where(r => r.CustomerId == userId
                             && r.Status != ReservationStatus.CancelledByCustomer
                             && r.Status != ReservationStatus.CancelledByRestaurant)
                    .Select(r => r.PackageId)
                    .ToListAsync()
                : new List<int>();

            var now = DateTime.Now;
            var activePackages = await _context.FoodPackages
                .Where(p => p.RestaurantId == id
                         && p.Status == PackageStatus.Active
                         && p.RemainingQuantity > 0
                         && p.PickupEndTime > now)
                .OrderBy(p => p.PickupEndTime)
                .Select(p => new PackageBrowseViewModel
                {
                    PackageId = p.PackageId,
                    PackageName = p.Name,
                    RestaurantId = p.RestaurantId,
                    RestaurantName = restaurant.Name,
                    City = restaurant.City,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    TotalQuantity = p.TotalQuantity,
                    RemainingQuantity = p.RemainingQuantity,
                    PickupStartTime = p.PickupStartTime,
                    PickupEndTime = p.PickupEndTime,
                    FoodType = p.FoodType,
                    ImagePath = p.ImagePath,
                    AlreadyReserved = reservedIds.Contains(p.PackageId)
                })
                .ToListAsync();

            // Only visible reviews (BR-015), most recent first.
            var reviews = await _context.Reviews
                .Where(r => r.RestaurantId == id && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new ReviewRowViewModel
                {
                    CustomerName = r.Customer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            foreach (var rev in reviews)
                rev.CustomerName = ShortenName(rev.CustomerName);

            var vm = new RestaurantProfileViewModel
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Category = restaurant.Category,
                Address = restaurant.Address,
                City = restaurant.City,
                Description = restaurant.Description,
                PhoneNumber = restaurant.PhoneNumber,
                OperatingHours = restaurant.OperatingHours,
                LogoPath = restaurant.LogoPath,
                CoverImagePath = restaurant.CoverImagePath,
                AverageRating = restaurant.AverageRating,
                TotalReviews = restaurant.TotalReviews,
                ActivePackages = activePackages,
                Reviews = reviews
            };

            return View(vm);
        }

        // Privacy: show first name + last initial (e.g., "Sara H.")
        private static string ShortenName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "Customer";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 1 ? parts[0] : $"{parts[0]} {parts[^1][0]}.";
        }
    }
}
