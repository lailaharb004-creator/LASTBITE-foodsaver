using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LastBiteNew.Models
{
    
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
