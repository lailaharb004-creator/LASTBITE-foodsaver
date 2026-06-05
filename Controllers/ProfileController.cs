using LastBiteNew.Models;
using LastBiteNew.ViewModels.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LastBiteNew.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return View(new CustomerProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                ExistingPhotoPath = user.ProfilePicturePath
            });
        }

        // POST: /Profile/UpdateProfile
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(CustomerProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                vm.Email = user.Email;
                vm.ExistingPhotoPath = user.ProfilePicturePath;
                return View("Index", vm);
            }

            user.FullName = vm.FullName;
            user.PhoneNumber = vm.PhoneNumber;

            if (vm.Photo is { Length: > 0 })
            {
                var path = await SaveImageAsync(vm.Photo);
                if (path != null) user.ProfilePicturePath = path;
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/ChangePassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the password fields and try again.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your password has been changed.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext)) return null;

            var dir = Path.Combine(_env.WebRootPath, "uploads", "users");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/uploads/users/{fileName}";
        }
    }
}
