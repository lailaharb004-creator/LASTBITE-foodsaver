namespace LastBiteNew.ViewModels.Admin
{
    public class AdminReviewRowViewModel
    {
        public int ReviewId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVisible { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
