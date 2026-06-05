using LastBiteNew.Models;
using LastBiteNew.ViewModels.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Restaurant.Controllers
{
    [Area("Restaurant")]
    [Authorize(Roles = "RestaurantOwner")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        private async Task<Models.Restaurant?> GetOwnerRestaurantAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == userId);
        }

        // GET: /Restaurant/Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            return View(new RestaurantProfileEditViewModel
            {
                RestaurantId = restaurant.RestaurantId,
                Name = restaurant.Name,
                Category = restaurant.Category,
                Address = restaurant.Address,
                City = restaurant.City,
                PhoneNumber = restaurant.PhoneNumber,
                Description = restaurant.Description,
                OperatingHours = restaurant.OperatingHours,
                ExistingLogoPath = restaurant.LogoPath,
                ExistingCoverPath = restaurant.CoverImagePath,
                Status = restaurant.Status
            });
        }

        // POST: /Restaurant/Profile/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RestaurantProfileEditViewModel vm)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
            {
                vm.ExistingLogoPath = restaurant.LogoPath;
                vm.ExistingCoverPath = restaurant.CoverImagePath;
                vm.Status = restaurant.Status;
                return View(vm);
            }

            restaurant.Name = vm.Name;
            restaurant.Category = vm.Category;
            restaurant.Address = vm.Address;
            restaurant.City = vm.City;
            restaurant.PhoneNumber = vm.PhoneNumber;
            restaurant.Description = vm.Description;
            restaurant.OperatingHours = vm.OperatingHours;

            if (vm.LogoImage is { Length: > 0 })
            {
                var path = await SaveImageAsync(vm.LogoImage);
                if (path != null) restaurant.LogoPath = path;
            }
            if (vm.CoverImage is { Length: > 0 })
            {
                var path = await SaveImageAsync(vm.CoverImage);
                if (path != null) restaurant.CoverImagePath = path;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your restaurant profile has been updated.";
            return RedirectToAction(nameof(Edit));
        }

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext)) return null;

            var dir = Path.Combine(_env.WebRootPath, "uploads", "restaurants");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/uploads/restaurants/{fileName}";
        }
    }
}
