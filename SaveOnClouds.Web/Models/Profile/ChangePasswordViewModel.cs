using System.ComponentModel.DataAnnotations;

namespace SaveOnClouds.Web.Models.Profile
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Your current password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Your new password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please re-type your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and its repeat do not match!")]
        public string NewPasswordRepeat { get; set; }

        public bool CanChangePassword { get; set; } = true;

        public bool HasAlreadyBeenUpdated { get; set; } = false;
    }
}