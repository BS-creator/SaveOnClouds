using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.Accounts
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public bool IsProcessed { get; set; }
    }
}