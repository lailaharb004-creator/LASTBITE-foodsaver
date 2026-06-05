using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LastBiteNew.Models
{
    public class Restaurant
    {
        [Key]
        public int RestaurantId { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public RestaurantCategory Category { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(256), EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? LogoPath { get; set; }

        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [StringLength(500)]
        public string? OperatingHours { get; set; }

        public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByAdminId { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        public int TotalReviews { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; } = null!;

        [ForeignKey("ApprovedByAdminId")]
        public ApplicationUser? ApprovedByAdmin { get; set; }

        public ICollection<FoodPackage> FoodPackages { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
