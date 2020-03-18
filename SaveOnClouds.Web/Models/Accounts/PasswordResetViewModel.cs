using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.Accounts
{
    public class PasswordResetViewModel
    {
        [Required(ErrorMessage = "User Id is missing!")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Token is missing!")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password repeat is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and its repeat must match.")]
        public string PasswordRepeat { get; set; }
    }
}