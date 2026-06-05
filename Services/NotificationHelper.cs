using LastBiteNew.Models;

namespace LastBiteNew.Services
{
    // Lightweight helper: queues a notification on the context.
    // The caller's existing SaveChangesAsync persists it (same transaction).
    public static class NotificationHelper
    {
        public static void Add(ApplicationDbContext ctx, string userId, string title,
            string message, NotificationType type, int? relatedId = null)
        {
            ctx.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
