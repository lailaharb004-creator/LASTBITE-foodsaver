using LastBiteNew.Models;
using System.ComponentModel.DataAnnotations;

namespace LastBiteNew.ViewModels.Owner
{
    public class PackageFormViewModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, Display(Name = "Original Price")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal OriginalPrice { get; set; }

        [Required, Display(Name = "Discounted Price")]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be between 0.01 and 9999.99")]
        public decimal DiscountedPrice { get; set; }

        [Required, Range(1, 20), Display(Name = "Total Quantity")]
        public int TotalQuantity { get; set; } = 1;

        [Required, Display(Name = "Pickup Start")]
        public DateTime PickupStartTime { get; set; }

        [Required, Display(Name = "Pickup End")]
        public DateTime PickupEndTime { get; set; }

        [Display(Name = "Food Type")]
        public FoodType FoodType { get; set; } = FoodType.Mixed;

        [StringLength(300), Display(Name = "Allergen Info")]
        public string? AllergenInfo { get; set; }

        [Display(Name = "Package Image")]
        public IFormFile? ImageFile { get; set; }

        // Populated on Edit so the current photo can be shown.
        public string? ExistingImagePath { get; set; }
    }

    public class PackageRowViewModel
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int TotalQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }
        public PackageStatus Status { get; set; }
        public FoodType FoodType { get; set; }
        public int TotalReservations { get; set; }

        public int SavingsPercent =>
            OriginalPrice > 0
                ? (int)Math.Round((1 - DiscountedPrice / OriginalPrice) * 100)
                : 0;
    }
}
