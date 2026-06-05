using LastBiteNew.Models;
using LastBiteNew.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string? role, string? q)
        {
            var users = await _userManager.Users.ToListAsync();

            // Precompute counts to avoid per-user round trips.
            var resCounts = await _context.Reservations
                .GroupBy(r => r.CustomerId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            var revCounts = await _context.Reviews
                .GroupBy(r => r.CustomerId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var rows = new List<UserRowViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                rows.Add(new UserRowViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "—",
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    Reservations = resCounts.TryGetValue(u.Id, out var rc) ? rc : 0,
                    Reviews = revCounts.TryGetValue(u.Id, out var vc) ? vc : 0
                });
            }

            if (!string.IsNullOrWhiteSpace(role))
                rows = rows.Where(r => r.Role == role).ToList();
            if (!string.IsNullOrWhiteSpace(q))
                rows = rows.Where(r => r.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)
                                    || r.Email.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            rows = rows.OrderByDescending(r => r.CreatedAt).ToList();

            ViewBag.CurrentRole = role;
            ViewBag.Query = q;
            return View(rows);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Don't ban admins or yourself.
            if (id == _userManager.GetUserId(User) || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Admin accounts cannot be banned.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"{user.FullName} has been banned.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"{user.FullName} has been reinstated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
