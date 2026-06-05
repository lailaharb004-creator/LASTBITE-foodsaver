using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LastBiteNew.Models
{
   

    public class FoodPackage
    {
        [Key]
        public int PackageId { get; set; }
        public int RestaurantId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(8,2)")]
        public decimal OriginalPrice { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal DiscountedPrice { get; set; }

        [Range(1, 20)]
        public int TotalQuantity { get; set; }

        public int RemainingQuantity { get; set; }

        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }

        public PackageStatus Status { get; set; } = PackageStatus.Active;
        public FoodType FoodType { get; set; } = FoodType.Mixed;

        [StringLength(300)]
        public string? AllergenInfo { get; set; }

        [StringLength(500)]
        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Restaurant Restaurant { get; set; } = null!;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
