using System.ComponentModel.DataAnnotations;

namespace LastBiteNew.ViewModels.Customer
{
    public class ReviewViewModel
    {
        public int ReservationId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please select a star rating.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string? Comment { get; set; }
    }
}
