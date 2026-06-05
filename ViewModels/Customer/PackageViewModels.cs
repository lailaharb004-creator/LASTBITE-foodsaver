using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Customer
{
    public class PackageBrowseViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public RestaurantCategory RestaurantCategory { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int TotalQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }
        public FoodType FoodType { get; set; }
        public string? AllergenInfo { get; set; }
        public string? ImagePath { get; set; }
        public decimal RestaurantRating { get; set; }
        public bool AlreadyReserved { get; set; }

        public int SavingsPercent => OriginalPrice > 0
            ? (int)Math.Round((1 - DiscountedPrice / OriginalPrice) * 100)
            : 0;

        public bool IsPickupToday => PickupStartTime.Date == DateTime.Now.Date;
        public bool IsUrgent => RemainingQuantity <= 2;
    }

    public class PackageDetailViewModel : PackageBrowseViewModel
    {
        public string RestaurantAddress { get; set; } = string.Empty;
        public string RestaurantPhone { get; set; } = string.Empty;
        public string? RestaurantOperatingHours { get; set; }
        public int TotalReviews { get; set; }
    }

    public class PackageFilterViewModel
    {
        public List<PackageBrowseViewModel> Packages { get; set; } = new();
        public List<string> Cities { get; set; } = new();
        public string? SelectedCity { get; set; }
        public string? SelectedFoodType { get; set; }
        public string? Search { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal PriceCeiling { get; set; } = 50;
        public string? SelectedPickup { get; set; }
        public string? SelectedSort { get; set; }
        public string? SelectedCategory { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ReservationConfirmViewModel
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public string RestaurantAddress { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }
    }

    public class MyReservationRowViewModel
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }

        public DateTime? CompletedAt { get; set; }
        public bool HasReview { get; set; }

        // BR-011: a pending reservation can be cancelled up to 1 hour before pickup starts.
        public bool CanCancel => Status == ReservationStatus.Pending
            && DateTime.Now < PickupStartTime.AddHours(-1);

        // BR-013/14: review a completed reservation once, within 48h of completion.
        public bool CanReview => Status == ReservationStatus.Completed
            && !HasReview
            && CompletedAt != null
            && DateTime.UtcNow <= CompletedAt.Value.AddHours(48);
    }

    // Home page model: featured packages slider + live impact stats.
    public class HomeViewModel
    {
        public List<PackageBrowseViewModel> FeaturedPackages { get; set; } = new();
        public int ActivePackagesCount { get; set; }
        public int RestaurantsCount { get; set; }
        public int MealsRescued { get; set; }
    }

    // Fallback visuals when a package has no uploaded image — keyed on food type.
    public static class PackageVisual
    {
        public static string Icon(FoodType t) => t switch
        {
            FoodType.Vegan => "🥗",
            FoodType.Vegetarian => "🥦",
            FoodType.ContainsMeat => "🍖",
            _ => "🍽️"
        };

        public static string Gradient(FoodType t) => t switch
        {
            FoodType.Vegan => "linear-gradient(135deg,#52B788,#1B4332)",
            FoodType.Vegetarian => "linear-gradient(135deg,#95D5B2,#52B788)",
            FoodType.ContainsMeat => "linear-gradient(135deg,#F4A261,#E76F51)",
            _ => "linear-gradient(135deg,#40916C,#2D6A4F)"
        };
    }
}
