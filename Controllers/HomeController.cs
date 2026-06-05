using System.Diagnostics;
using LastBiteNew.Models;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var availableQuery = _context.FoodPackages
                .Include(p => p.Restaurant)
                .Where(p => p.Status == PackageStatus.Active
                         && p.RemainingQuantity > 0
                         && p.PickupEndTime > now
                         && p.Restaurant.Status == RestaurantStatus.Approved);

            var featured = await availableQuery
                .OrderBy(p => p.PickupEndTime)
                .Take(3)
                .Select(p => new PackageBrowseViewModel
                {
                    PackageId = p.PackageId,
                    PackageName = p.Name,
                    RestaurantName = p.Restaurant.Name,
                    City = p.Restaurant.City,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    TotalQuantity = p.TotalQuantity,
                    RemainingQuantity = p.RemainingQuantity,
                    PickupStartTime = p.PickupStartTime,
                    PickupEndTime = p.PickupEndTime,
                    FoodType = p.FoodType,
                    ImagePath = p.ImagePath,
                    RestaurantRating = p.Restaurant.AverageRating
                })
                .ToListAsync();

            var vm = new HomeViewModel
            {
                FeaturedPackages = featured,
                ActivePackagesCount = await availableQuery.CountAsync(),
                RestaurantsCount = await _context.Restaurants
                    .CountAsync(r => r.Status == RestaurantStatus.Approved),
                MealsRescued = await _context.Reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .SumAsync(r => (int?)r.Quantity) ?? 0
            };

            return View(vm);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
