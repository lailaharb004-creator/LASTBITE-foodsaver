using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Admin
{
    public class RestaurantRowViewModel
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public RestaurantCategory Category { get; set; }
        public string City { get; set; } = string.Empty;
        public RestaurantStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPackages { get; set; }
        public decimal AverageRating { get; set; }
    }
}
