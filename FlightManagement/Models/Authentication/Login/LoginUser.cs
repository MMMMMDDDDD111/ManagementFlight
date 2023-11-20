using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlightManagement.Models.Authentication.Login
{
    [Table("Login")]
    public class LoginUser
    {
        [Key]
        [Required(ErrorMessage ="User name is required")]
        public string? Username { get; set;}
        [JsonIgnore]
        public string? Password { get; set;}
    }
    public class ChangePassword
    {
        [Required(ErrorMessage = "User name is required")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Current password is required")]
        public string? CurrentPassword { get; set; }
        [Required(ErrorMessage = "New password is required")]
        public string? NewPassword { get; set; }
        [Required(ErrorMessage = "Comfirm new password is required")]
        public string? ConfirmNewPassword { get; set; }
    }
    public class ResetPassword
    {
        public string? Username { get; set; }
        public string? Resetpassword { get; set; }
    }
    public class Profile 
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public int? PhoneNumber { get; set; }
        public int? Address { get; set; }
        public IFormFile? Image { get; set; }
    }

}
