using LastBiteNew.Models;
using LastBiteNew.Services;
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
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PackagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext)) return null;

            var dir = Path.Combine(_env.WebRootPath, "uploads", "packages");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/uploads/packages/{fileName}";
        }

        private async Task<Models.Restaurant?> GetOwnerRestaurantAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Restaurants.FirstOrDefaultAsync(r => r.OwnerId == userId);
        }

        // GET: /Restaurant/Packages/Index
        public async Task<IActionResult> Index(string? status)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            if (restaurant.Status != RestaurantStatus.Approved)
            {
                TempData["Error"] = "Your restaurant must be approved before you can manage packages.";
                return RedirectToAction("Index", "Dashboard");
            }

            var query = _context.FoodPackages
                .Where(p => p.RestaurantId == restaurant.RestaurantId);

            if (Enum.TryParse<PackageStatus>(status, out var parsedStatus))
                query = query.Where(p => p.Status == parsedStatus);

            var packages = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PackageRowViewModel
                {
                    PackageId = p.PackageId,
                    Name = p.Name,
                    OriginalPrice = p.OriginalPrice,
                    DiscountedPrice = p.DiscountedPrice,
                    TotalQuantity = p.TotalQuantity,
                    RemainingQuantity = p.RemainingQuantity,
                    PickupStartTime = p.PickupStartTime,
                    PickupEndTime = p.PickupEndTime,
                    Status = p.Status,
                    FoodType = p.FoodType,
                    TotalReservations = p.Reservations.Count()
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(packages);
        }

        // GET: /Restaurant/Packages/Create
        public async Task<IActionResult> Create()
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null || restaurant.Status != RestaurantStatus.Approved)
                return RedirectToAction("Index", "Dashboard");

            return View(new PackageFormViewModel
            {
                PickupStartTime = DateTime.Now.AddHours(1),
                PickupEndTime = DateTime.Now.AddHours(3)
            });
        }

        // POST: /Restaurant/Packages/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PackageFormViewModel vm)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null || restaurant.Status != RestaurantStatus.Approved)
                return RedirectToAction("Index", "Dashboard");

            if (vm.OriginalPrice > 0 && vm.DiscountedPrice > vm.OriginalPrice * 0.70m)
                ModelState.AddModelError(nameof(vm.DiscountedPrice),
                    "Discounted price must be at least 30% lower than the original price (BR-004).");

            if (vm.PickupEndTime <= vm.PickupStartTime)
                ModelState.AddModelError(nameof(vm.PickupEndTime),
                    "Pickup end time must be after start time.");

            if (vm.PickupStartTime < DateTime.Now)
                ModelState.AddModelError(nameof(vm.PickupStartTime),
                    "Pickup start time cannot be in the past.");

            if (!ModelState.IsValid) return View(vm);

            var package = new FoodPackage
            {
                RestaurantId = restaurant.RestaurantId,
                Name = vm.Name,
                Description = vm.Description,
                OriginalPrice = vm.OriginalPrice,
                DiscountedPrice = vm.DiscountedPrice,
                TotalQuantity = vm.TotalQuantity,
                RemainingQuantity = vm.TotalQuantity,
                PickupStartTime = vm.PickupStartTime,
                PickupEndTime = vm.PickupEndTime,
                FoodType = vm.FoodType,
                AllergenInfo = vm.AllergenInfo,
                Status = PackageStatus.Active
            };

            if (vm.ImageFile is { Length: > 0 })
                package.ImagePath = await SaveImageAsync(vm.ImageFile);

            _context.FoodPackages.Add(package);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Package \"{package.Name}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Restaurant/Packages/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var package = await _context.FoodPackages
                .FirstOrDefaultAsync(p => p.PackageId == id && p.RestaurantId == restaurant.RestaurantId);

            if (package == null) return NotFound();

            if (package.Status is PackageStatus.Cancelled or PackageStatus.Expired)
            {
                TempData["Error"] = "Cancelled or expired packages cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(new PackageFormViewModel
            {
                Name = package.Name,
                Description = package.Description,
                OriginalPrice = package.OriginalPrice,
                DiscountedPrice = package.DiscountedPrice,
                TotalQuantity = package.TotalQuantity,
                PickupStartTime = package.PickupStartTime,
                PickupEndTime = package.PickupEndTime,
                FoodType = package.FoodType,
                AllergenInfo = package.AllergenInfo,
                ExistingImagePath = package.ImagePath
            });
        }

        // POST: /Restaurant/Packages/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PackageFormViewModel vm)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var package = await _context.FoodPackages
                .FirstOrDefaultAsync(p => p.PackageId == id && p.RestaurantId == restaurant.RestaurantId);

            if (package == null) return NotFound();

            if (package.Status is PackageStatus.Cancelled or PackageStatus.Expired)
            {
                TempData["Error"] = "Cancelled or expired packages cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.OriginalPrice > 0 && vm.DiscountedPrice > vm.OriginalPrice * 0.70m)
                ModelState.AddModelError(nameof(vm.DiscountedPrice),
                    "Discounted price must be at least 30% lower than the original price (BR-004).");

            if (vm.PickupEndTime <= vm.PickupStartTime)
                ModelState.AddModelError(nameof(vm.PickupEndTime),
                    "Pickup end time must be after start time.");

            if (!ModelState.IsValid)
            {
                vm.ExistingImagePath = package.ImagePath;
                return View(vm);
            }

            // Adjust remaining quantity proportionally when total changes
            int diff = vm.TotalQuantity - package.TotalQuantity;
            package.RemainingQuantity = Math.Max(0, package.RemainingQuantity + diff);

            package.Name = vm.Name;
            package.Description = vm.Description;
            package.OriginalPrice = vm.OriginalPrice;
            package.DiscountedPrice = vm.DiscountedPrice;
            package.TotalQuantity = vm.TotalQuantity;
            package.PickupStartTime = vm.PickupStartTime;
            package.PickupEndTime = vm.PickupEndTime;
            package.FoodType = vm.FoodType;
            package.AllergenInfo = vm.AllergenInfo;

            if (vm.ImageFile is { Length: > 0 })
            {
                var newPath = await SaveImageAsync(vm.ImageFile);
                if (newPath != null) package.ImagePath = newPath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Package \"{package.Name}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Restaurant/Packages/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var restaurant = await GetOwnerRestaurantAsync();
            if (restaurant == null) return RedirectToAction("Index", "Dashboard");

            var package = await _context.FoodPackages
                .FirstOrDefaultAsync(p => p.PackageId == id && p.RestaurantId == restaurant.RestaurantId);

            if (package == null) return NotFound();

            if (package.Status is PackageStatus.Cancelled or PackageStatus.Expired)
            {
                TempData["Error"] = "Package is already cancelled or expired.";
                return RedirectToAction(nameof(Index));
            }

            // BR-005: cancel all pending reservations and notify those customers.
            var pending = await _context.Reservations
                .Where(r => r.PackageId == package.PackageId && r.Status == ReservationStatus.Pending)
                .ToListAsync();

            foreach (var res in pending)
            {
                res.Status = ReservationStatus.CancelledByRestaurant;
                NotificationHelper.Add(_context, res.CustomerId, "Reservation cancelled",
                    $"Unfortunately, \"{package.Name}\" was cancelled by the restaurant. Your reservation {res.ReservationCode} has been cancelled.",
                    NotificationType.PackageCancellation, package.PackageId);
            }

            package.Status = PackageStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = pending.Count > 0
                ? $"Package \"{package.Name}\" cancelled. {pending.Count} pending reservation(s) were cancelled and customers notified."
                : $"Package \"{package.Name}\" has been cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}
