using LastBiteNew.Models;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    [Route("Packages")]
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PackagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Packages
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? city, string? foodType, string? search, decimal? maxPrice, string? pickup, string? sort, string? category, int page = 1)
        {
            var userId = _userManager.GetUserId(User);

            var reservedIds = userId != null
                ? await _context.Reservations
                    .Where(r => r.CustomerId == userId
                             && r.Status != ReservationStatus.CancelledByCustomer
                             && r.Status != ReservationStatus.CancelledByRestaurant)
                    .Select(r => r.PackageId)
                    .ToListAsync()
                : new List<int>();

            var query = _context.FoodPackages
                .Include(p => p.Restaurant)
                .Where(p => p.Status == PackageStatus.Active
                         && p.RemainingQuantity > 0
                         && p.PickupEndTime > DateTime.Now
                         && p.Restaurant.Status == RestaurantStatus.Approved);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.Restaurant.City == city);

            if (Enum.TryParse<FoodType>(foodType, out var ft))
                query = query.Where(p => p.FoodType == ft);

            if (Enum.TryParse<RestaurantCategory>(category, out var cat))
                query = query.Where(p => p.Restaurant.Category == cat);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search)
                                      || p.Restaurant.Name.Contains(search));

            // Pickup-time filter
            var nowTime = DateTime.Now;
            var todayStart = DateTime.Today;
            query = pickup switch
            {
                "now" => query.Where(p => p.PickupStartTime <= nowTime && p.PickupEndTime > nowTime),
                "today" => query.Where(p => p.PickupStartTime >= todayStart && p.PickupStartTime < todayStart.AddDays(1)),
                "tomorrow" => query.Where(p => p.PickupStartTime >= todayStart.AddDays(1) && p.PickupStartTime < todayStart.AddDays(2)),
                "week" => query.Where(p => p.PickupStartTime >= todayStart && p.PickupStartTime < todayStart.AddDays(7)),
                _ => query
            };

            // Highest discounted price among available packages — sets the slider's upper bound.
            var priceCeiling = await _context.FoodPackages
                .Where(p => p.Status == PackageStatus.Active
                         && p.RemainingQuantity > 0
                         && p.PickupEndTime > DateTime.Now
                         && p.Restaurant.Status == RestaurantStatus.Approved)
                .Select(p => (decimal?)p.DiscountedPrice)
                .MaxAsync() ?? 50m;
            priceCeiling = Math.Max(1m, Math.Ceiling(priceCeiling));

            if (maxPrice.HasValue)
                query = query.Where(p => p.DiscountedPrice <= maxPrice.Value);

            query = sort switch
            {
                "price" => query.OrderBy(p => p.DiscountedPrice),
                "savings" => query.OrderByDescending(p => p.OriginalPrice - p.DiscountedPrice),
                "rating" => query.OrderByDescending(p => p.Restaurant.AverageRating),
                _ => query.OrderBy(p => p.PickupEndTime)
            };

            const int pageSize = 12;
            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var packages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PackageBrowseViewModel
                {
                    PackageId = p.PackageId,
                    PackageName = p.Name,
                    Description = p.Description,
                    RestaurantId = p.RestaurantId,
                    RestaurantName = p.Restaurant.Name,
                    RestaurantCategory = p.Restaurant.Category,
                    City = p.Restaurant.City,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    TotalQuantity = p.TotalQuantity,
                    RemainingQuantity = p.RemainingQuantity,
                    PickupStartTime = p.PickupStartTime,
                    PickupEndTime = p.PickupEndTime,
                    FoodType = p.FoodType,
                    AllergenInfo = p.AllergenInfo,
                    ImagePath = p.ImagePath,
                    RestaurantRating = p.Restaurant.AverageRating,
                    AlreadyReserved = reservedIds.Contains(p.PackageId)
                })
                .ToListAsync();

            var cities = await _context.Restaurants
                .Where(r => r.Status == RestaurantStatus.Approved)
                .Select(r => r.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var vm = new PackageFilterViewModel
            {
                Packages = packages,
                Cities = cities,
                SelectedCity = city,
                SelectedFoodType = foodType,
                Search = search,
                MaxPrice = maxPrice,
                PriceCeiling = priceCeiling,
                SelectedPickup = pickup,
                SelectedSort = sort,
                SelectedCategory = category,
                Page = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        // GET: /Packages/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var package = await _context.FoodPackages
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.PackageId == id
                                       && p.Restaurant.Status == RestaurantStatus.Approved);

            if (package == null) return NotFound();

            var alreadyReserved = userId != null && await _context.Reservations
                .AnyAsync(r => r.PackageId == id
                            && r.CustomerId == userId
                            && r.Status != ReservationStatus.CancelledByCustomer
                            && r.Status != ReservationStatus.CancelledByRestaurant);

            var vm = new PackageDetailViewModel
            {
                PackageId = package.PackageId,
                PackageName = package.Name,
                Description = package.Description,
                RestaurantId = package.RestaurantId,
                RestaurantName = package.Restaurant.Name,
                RestaurantCategory = package.Restaurant.Category,
                City = package.Restaurant.City,
                RestaurantAddress = package.Restaurant.Address,
                RestaurantPhone = package.Restaurant.PhoneNumber,
                RestaurantOperatingHours = package.Restaurant.OperatingHours,
                OriginalPrice = package.OriginalPrice,
                DiscountedPrice = package.DiscountedPrice,
                TotalQuantity = package.TotalQuantity,
                RemainingQuantity = package.RemainingQuantity,
                PickupStartTime = package.PickupStartTime,
                PickupEndTime = package.PickupEndTime,
                FoodType = package.FoodType,
                AllergenInfo = package.AllergenInfo,
                ImagePath = package.ImagePath,
                RestaurantRating = package.Restaurant.AverageRating,
                TotalReviews = package.Restaurant.TotalReviews,
                AlreadyReserved = alreadyReserved
            };

            return View(vm);
        }
    }
}
