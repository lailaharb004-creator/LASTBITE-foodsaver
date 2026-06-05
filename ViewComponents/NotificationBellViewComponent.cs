using LastBiteNew.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationBellViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            if (string.IsNullOrEmpty(userId))
                return Content(string.Empty);

            var recent = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.UnreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return View(recent);
        }
    }
}
