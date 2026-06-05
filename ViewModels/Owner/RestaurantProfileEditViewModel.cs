using System.ComponentModel.DataAnnotations;
using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Owner
{
    public class RestaurantProfileEditViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public RestaurantCategory Category { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Operating Hours")]
        public string? OperatingHours { get; set; }

        [Display(Name = "Logo")]
        public IFormFile? LogoImage { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }

        public string? ExistingLogoPath { get; set; }
        public string? ExistingCoverPath { get; set; }

        public RestaurantStatus Status { get; set; }
        public int RestaurantId { get; set; }
    }
}
