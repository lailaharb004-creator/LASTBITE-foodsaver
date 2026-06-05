using LastBiteNew.Models;
using LastBiteNew.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context) => _context = context;

        // GET: /Admin/Reports
        public async Task<IActionResult> Index()
        {
            var completed = _context.Reservations.Where(r => r.Status == ReservationStatus.Completed);

            var vm = new ReportsViewModel
            {
                TotalRevenue = await completed.SumAsync(r => (decimal?)r.TotalPrice) ?? 0,
                TotalCommission = await completed.SumAsync(r => (decimal?)r.CommissionAmount) ?? 0,
                MealsRescued = await completed.SumAsync(r => (int?)r.Quantity) ?? 0,
                TotalReservations = await _context.Reservations.CountAsync(),

                PendingCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending),
                CompletedCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Completed),
                CancelledCount = await _context.Reservations.CountAsync(r =>
                    r.Status == ReservationStatus.CancelledByCustomer || r.Status == ReservationStatus.CancelledByRestaurant),
                NoShowCount = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.NoShow),

                ApprovedRestaurants = await _context.Restaurants.CountAsync(r => r.Status == RestaurantStatus.Approved),
                TotalUsers = await _context.Users.CountAsync(),
                TotalPackages = await _context.FoodPackages.CountAsync(),

                TopRestaurants = await _context.Reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .GroupBy(r => r.FoodPackage.Restaurant.Name)
                    .Select(g => new TopRestaurantRow
                    {
                        Name = g.Key,
                        CompletedPickups = g.Count(),
                        Revenue = g.Sum(x => x.TotalPrice)
                    })
                    .OrderByDescending(g => g.Revenue)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
