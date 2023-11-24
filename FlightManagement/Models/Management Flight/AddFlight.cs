using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Authentication.Signup;
using FlightManagement.NewFolder;
using MessagePack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
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
        public string? Status { get; set; }
        [ForeignKey("Groups")]
        public int GroupId { get; set; }
        public Groups Groups { get; set; }
        public string? Creator { get; set; }
        public DateTime? UpdateDate { get; set; }
        [ForeignKey("UpdatedVersion")]
        public int UpdateVersionId { get; set; }
        public ICollection<UpdateVersion> UpdateVersions { get; set; } = new List<UpdateVersion>();
        public ICollection<PreviousVersion> PreviousVersions { get; set; } = new List<PreviousVersion>();
    }

    [Table("Group")]
    public class Groups
    {
        [Key]
        public int GroupId { get; set; }
        [Required(ErrorMessage = "GroupName is required.")]
        [RegularExpression("^(pilot|crew)$", ErrorMessage = "GroupName must be 'pilot' or 'crew'.")]
        public string? GroupName { get; set; }
        public Permission? Permissions { get; set; }
        [ForeignKey("Creator")]
        public string? Creator { get; set; }
        public ICollection<IdentityUser>? Members { get; set; } = new List<IdentityUser>();
    }

    public class AddFlightDTO
    {
        public int FlightId { get; set; }
        public string FlightNo { get; set; }
        public DateTime? Date { get; set; }
        public string Route { get; set; }
        public int? TotalDocument { get; set; }
        public List<DocumentDTO> Documents { get; set; } = new List<DocumentDTO>();
    }

    public class DocumentDTO
    {
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Creator { get; set; }
        public string DocumentVersion { get; set; } 
    }
    public class DocumentInfoDTO
    {
        public int ID { get; set; }
        public int IdFlight { get; set; }
        public int GroupId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
    }
    
    public class DocsList
    {
        public string DocumentName { get; set; }
        public string Type { get; set; }
        public DateTime CreateDate { get; set; }
        public string Creator { get; set; }
        public string FlightNO { get; set; }
    }
    public class ViewDoc
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? Creator { get; set; }
        public Permission? Permissions { get; set; }
        public List<UpdateVersionDTO>? UpdatedVersions { get; set; }

    }
    public class UpdateVersion
    {
        [Key]
        public int Id { get; set; }
        public int DocID { get; set; }
        public List<PreviousVersion> PreviousVersions { get; set; } = new List<PreviousVersion>();
        public string Date { get; set; }
        public DocumentInformation DocumentInformation { get; set; }
    }
    
    public class UpdateVersionDTO
    {
        public int Id { get; set; }
        public int DocID { get; set; }
        public List<PreviousVersionDTO> PreviousVersions { get; set; } = new List<PreviousVersionDTO>();
        public string Date { get; set; }
    }

    public class PreviousVersion
    {
        [Key]
        public int Id { get; set; }
        public string Version { get; set; }
        [ForeignKey("UpdateVersionId")]
        public int UpdateVersionId { get; set; }
        public UpdateVersion UpdateVersion { get; set; }
    }
    public class PreviousVersionDTO
    {
        public int Id { get; set; }
        public string Version { get; set; }
    }
    public class FlightDTO
    {
        public int FlightId { get; set; }
        [Required]
        public string? Flightno { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public string? Pointofloding { get; set; }
        [Required]
        public string? Pointofunloading { get; set; }
    }   
    public class GroupDto
    {
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public Permission? Permissions { get; set; }
        public string? Creator { get; set; }
        public List<string>? Username { get; set; } = new List<string>();
    }
    public class GroupDTO
    {
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? Member { get; set; }
        public Permission? Permissions { get; set; }
        public string? Creator { get; set; }
        public List<string>? Username { get; set; } = new List<string>();
    }
    public class RecentlyActivityDTO
    {
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string FlightNo { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Creator { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class CurrentFlightDTO
    {
        public int FlightId { get; set; }
        public DateTime? DepartureTime { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public List<DocumentInfoDTO>? SentFiles { get; set; }
        public List<DocumentInfoDTO>? ReturnedFiles { get; set; }
    }

    public class DashboardDTO
    {
        public List<RecentlyActivityDTO> RecentlyActivities { get; set; }
        public List<CurrentFlightDTO> CurrentFlights { get; set; }
    }


}


