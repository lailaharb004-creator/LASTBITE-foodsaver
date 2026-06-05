using LastBiteNew.Models;
using LastBiteNew.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
            => _context = context;

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalApprovedRestaurants = await _context.Restaurants
                    .CountAsync(r => r.Status == RestaurantStatus.Approved),

                PendingApprovals = await _context.Restaurants
                    .CountAsync(r => r.Status == RestaurantStatus.Pending),

                TotalCustomers = await _context.Users
                    .CountAsync(),

                TotalReservations = await _context.Reservations
                    .CountAsync(),

                CompletedReservations = await _context.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.Completed),

                TotalCommissionEarned = await _context.Reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .SumAsync(r => r.CommissionAmount),

                MealsSaved = await _context.Reservations
                    .Where(r => r.Status == ReservationStatus.Completed)
                    .SumAsync(r => r.Quantity),

                PendingRestaurants = await _context.Restaurants
                    .AsNoTracking()
                    .Where(r => r.Status == RestaurantStatus.Pending)
                    .Include(r => r.Owner)
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new RestaurantRowViewModel
                    {
                        RestaurantId = r.RestaurantId,
                        Name = r.Name,
                        OwnerName = r.Owner.FullName,
                        OwnerEmail = r.Owner.Email ?? "",
                        Category = r.Category,
                        City = r.City,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt
                    }).ToListAsync()
            };

            return View(vm);
        }
    }
}