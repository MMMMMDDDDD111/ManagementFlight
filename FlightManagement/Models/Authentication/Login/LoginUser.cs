using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightManagement.Models.Authentication.Login
{
    public class LoginUser
    {
        [Key]
        [Required(ErrorMessage ="User name is required")]
        public string? Username { get; set;}
        [JsonIgnore]
        public string? Password { get; set;}
    }
}
