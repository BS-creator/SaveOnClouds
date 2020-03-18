using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.Accounts
{
    public class SignUpViewModel
    {
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password, ErrorMessage = "Incorrect or missing password.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password, ErrorMessage = "Incorrect or missing password repeat.")]
        [Compare("Password",
            ErrorMessage = "You have entered two different passwords. The password and its repeat must be identical.")]
        public string PasswordRepeat { get; set; }

        public string FullName { get; set; }
        public string CompanyName { get; set; }

        [DataType(DataType.PhoneNumber)] 
        public string Phone { get; set; }
        public string TimeZone { get; set; }
        public string FullAddress { get; set; }

        public bool PrivacyPolicyAgreed { get; set; }

        public bool UserAlreadyExists { get; set; } = false;
    }
}