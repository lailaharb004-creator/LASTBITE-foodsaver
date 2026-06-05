using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Customer
{
    public class RestaurantProfileViewModel
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public RestaurantCategory Category { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OperatingHours { get; set; }
        public string? LogoPath { get; set; }
        public string? CoverImagePath { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public List<PackageBrowseViewModel> ActivePackages { get; set; } = new();
        public List<ReviewRowViewModel> Reviews { get; set; } = new();
    }

    public class ReviewRowViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
