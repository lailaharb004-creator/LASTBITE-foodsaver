using Microsoft.AspNetCore.Identity;
namespace LastBiteNew.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Restaurant? Restaurant { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}