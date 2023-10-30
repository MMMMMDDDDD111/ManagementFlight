using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Authentication.Signup;
using MessagePack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit.Sdk;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FlightManagement.Models.Management_Flight
{
    [Table("AddFlight")]
    public class AddFlight
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string? Flightno { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public string? Pointofloding { get; set; }
        [Required]
        public string? Pointofunloading { get; set; }
    }

    [Table("DocumentInfo")]
    public class DocumentInformation
    {
        public int Id { get; set; }
        public string? Documentname { get; set; }
        public string? Documenttype { get; set; }
        public string? Documentversion { get; set; }
        public string? Note { get; set; }
        public string? RoleId { get; set; }
        public IdentityRole? Role { get; set; }
        public string? FileName { get; set; }
    }
    [Table("Permission")]
    public class Permission
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "GroupName is required.")]
        [RegularExpression("^(pilot|crew)$", ErrorMessage = "GroupName must be 'pilot' or 'crew'.")]
        public string? GroupName { get; set; }
        public int? Members { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? Note { get; set; } 
        public string? CreatorUsername { get; set; }
        public LoginUser? Creator { get; set; }
    }
}


