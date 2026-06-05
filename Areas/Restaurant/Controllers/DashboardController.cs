using LastBiteNew.Models;
using LastBiteNew.ViewModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Restaurant.Controllers
{
    [Area("Restaurant")]
    [Authorize(Roles = "RestaurantOwner")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.OwnerId == userId);

            if (restaurant == null)
                return RedirectToAction("Index", "Home");

            var vm = new OwnerDashboardViewModel { Restaurant = restaurant };

            if (restaurant.Status == RestaurantStatus.Approved)
            {
                var packages = await _context.FoodPackages
                    .Where(p => p.RestaurantId == restaurant.RestaurantId)
                    .ToListAsync();

                var packageIds = packages.Select(p => p.PackageId).ToList();

                var reservations = await _context.Reservations
                    .Include(r => r.Customer)
                    .Include(r => r.FoodPackage)
                    .Where(r => packageIds.Contains(r.PackageId))
                    .OrderByDescending(r => r.ReservedAt)
                    .ToListAsync();

                vm.ActivePackages = packages.Count(p => p.Status == PackageStatus.Active);
                vm.TotalPackages = packages.Count;
                vm.TotalReservations = reservations.Count;
                vm.CompletedReservations = reservations.Count(r => r.Status == ReservationStatus.Completed);
                vm.TotalRevenue = reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .Sum(r => r.TotalPrice);
                vm.MealsSaved = reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .Sum(r => r.Quantity);
                vm.RecentReservations = reservations
                    .Take(10)
                    .Select(r => new RecentReservationRow
                    {
                        ReservationCode = r.ReservationCode,
                        CustomerName = r.Customer.FullName,
                        PackageName = r.FoodPackage.Name,
                        Quantity = r.Quantity,
                        TotalPrice = r.TotalPrice,
                        Status = r.Status,
                        ReservedAt = r.ReservedAt
                    })
                    .ToList();
            }

            return View(vm);
        }
    }
}
