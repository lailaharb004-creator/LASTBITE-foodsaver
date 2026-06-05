using Microsoft.AspNetCore.Mvc;

using LastBiteNew.Models;
using LastBiteNew.Services;
using LastBiteNew.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace LastBiteNew.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _env = env;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View(new AuthViewModel { ActiveTab = "login" });

        // POST: /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", new AuthViewModel { Login = model, ActiveTab = "login" });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && !existingUser.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This account has been suspended. Please contact support.");
                return View("Login", new AuthViewModel { Login = model, ActiveTab = "login" });
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View("Login", new AuthViewModel { Login = model, ActiveTab = "login" });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (await _userManager.IsInRoleAsync(user!, "Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (await _userManager.IsInRoleAsync(user!, "RestaurantOwner"))
                return RedirectToAction("Index", "Dashboard", new { area = "Restaurant" });

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/RegisterCustomer
        [HttpGet]
        public IActionResult RegisterCustomer() => View("Login", new AuthViewModel { ActiveTab = "register" });

        // POST: /Account/RegisterCustomer
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCustomer([Bind(Prefix = "Register")] RegisterCustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", new AuthViewModel { Register = model, ActiveTab = "register" });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View("Login", new AuthViewModel { Register = model, ActiveTab = "register" });
            }

            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/RegisterRestaurant
        [HttpGet]
        public IActionResult RegisterRestaurant() => View();

        // POST: /Account/RegisterRestaurant
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterRestaurant(RegisterRestaurantViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "RestaurantOwner");

            // Save uploaded restaurant image (optional) to wwwroot/uploads/restaurants.
            string? logoPath = null;
            if (model.LogoImage is { Length: > 0 })
            {
                var ext = Path.GetExtension(model.LogoImage.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                if (allowed.Contains(ext))
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "restaurants");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await model.LogoImage.CopyToAsync(stream);
                    logoPath = $"/uploads/restaurants/{fileName}";
                }
            }

            var restaurant = new Restaurant
            {
                OwnerId = user.Id,
                Name = model.RestaurantName,
                Category = model.Category,
                Address = model.Address,
                City = model.City,
                PhoneNumber = model.PhoneNumber,
                Description = model.Description,
                LogoPath = logoPath,
                Status = RestaurantStatus.Pending
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Notify all admins of the pending application (FR-01).
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
                NotificationHelper.Add(_context, admin.Id, "New restaurant application",
                    $"\"{restaurant.Name}\" applied and is awaiting approval.",
                    NotificationType.RestaurantApproval, restaurant.RestaurantId);
            if (admins.Count > 0)
                await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard", new { area = "Restaurant" });
        }

        // POST: /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => RedirectToAction("Login");
    }
}
