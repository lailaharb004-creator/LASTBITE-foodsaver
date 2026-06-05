using System.ComponentModel.DataAnnotations;

namespace LastBiteNew.ViewModels.Account
{

    public class RegisterCustomerViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
