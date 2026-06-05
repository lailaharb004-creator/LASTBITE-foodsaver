using LastBiteNew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notifications  — list, and mark unread as read on view
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var items = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var unread = items.Where(n => !n.IsRead).ToList();
            if (unread.Count > 0)
            {
                unread.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return View(items);
        }
    }
}
