using LastBiteNew.Models;
using System.ComponentModel.DataAnnotations;
namespace LastBiteNew.ViewModels.Account
{
   

    public class RegisterRestaurantViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Your Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Restaurant Name")]
        public string RestaurantName { get; set; } = string.Empty;

        [Required]
        public RestaurantCategory Category { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, Phone]
        [Display(Name = "Business Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Restaurant Image")]
        public IFormFile? LogoImage { get; set; }
    }
}
