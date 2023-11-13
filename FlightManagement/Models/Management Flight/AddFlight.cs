using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Authentication.Signup;
using FlightManagement.NewFolder;
using MessagePack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Xunit.Sdk;
using static FlightManagement.NewFolder.EnumExtensions;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace FlightManagement.Models.Management_Flight
{
    [Table("AddFlight")]
    public class AddFlight
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int FlightId { get; set; }
        [Required]
        public string? Flightno { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public string? Pointofloding { get; set; }
        [Required]
        public string? Pointofunloading { get; set; }

        public ICollection<DocumentInformation>? DocumentInformation { get; set; }
    }

    [Table("DocumentInfo")]
    public class DocumentInformation
    {
        public int Id { get; set; }
        public string? Documentname { get; set; }
        public string? Documenttype { get; set; }
        public string? Documentversion { get; set; } = "1.0";
        public string? Note { get; set; }
        public string? FileName { get; set; }
        [ForeignKey("AddFlightId")]
        public int IdFlight { get; set; }
        [JsonIgnore]
        public AddFlight? AddFlight { get; set; }
        [ForeignKey("Groups")]
        public int GroupId { get; set; }
        public Groups Groups { get; set; }
    }
  
    [Table("Group")]
    public class Groups
    {
        [Key]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "GroupName is required.")]
        [RegularExpression("^(pilot|crew)$", ErrorMessage = "GroupName must be 'pilot' or 'crew'.")]
        public string? GroupName { get; set; }
        public int? Member { get; set; }
        public Permission? Permissions { get; set; }
        [ForeignKey("Creator")]
        public string? Creator { get; set; }
        public ICollection<LoginUser>? Members { get; set; }
    }
}


