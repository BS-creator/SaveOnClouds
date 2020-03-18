using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.Accounts
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "Your username is your email address. It must be provided!")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password must be provided.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public bool FirstTimeLogin { get; set; }
        public string Error { get; set; }
    }
}