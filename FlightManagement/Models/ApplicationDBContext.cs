using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static FlightManagement.Controllers.GroupsController;

namespace FlightManagement.Models
{
   public class ApplicationDBContext : IdentityDbContext<IdentityUser>
    {
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) 
        {
            Addflights = Set<AddFlight>();
            DocumentInfo = Set<DocumentInformation>();
            Group = Set<Groups>();
        }


    public DbSet<AddFlight> Addflights { get; set; }
    public DbSet<DocumentInformation> DocumentInfo { get; set; }
    public DbSet<Groups> Group { get; set; }
    public DbSet<LoginUser> loginUsers { get; set; }
  

        protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        SeedRoles(builder);
        builder.Entity<AddFlight>()
        .Property(a => a.Date)
        .HasColumnType("date");

        builder.Entity<AddFlight>()
         .HasMany(a => a.DocumentInformation)
         .WithOne(di => di.AddFlight)
         .HasForeignKey(di => di.Id);

        builder.Entity<AddFlight>()
          .HasMany(addFlight => addFlight.DocumentInformation)
          .WithOne(document => document.AddFlight)
          .HasForeignKey(document => document.IdFlight);


            builder.Entity<Groups>(entity =>
            {
                entity.ToTable("Group");
                entity.HasKey(g => g.GroupId);
                entity.Property(g => g.GroupName)
                    .IsRequired()
                    .HasAnnotation("RegularExpression", "^(pilot|crew)$")
                    .HasAnnotation("ErrorMessage", "GroupName must be 'pilot' or 'crew");
                entity.Property(g => g.Member);

            });

        }

    public static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
            new IdentityRole { Id = "2", Name = "Employee", ConcurrencyStamp = "2", NormalizedName = "Employee" },
            new IdentityRole {Id = "3", Name = "Pilot", ConcurrencyStamp = "3", NormalizedName = "Pilot" },
            new IdentityRole {Id = "4", Name = "Crew", ConcurrencyStamp = "4", NormalizedName = "Crew" }
        );
    }
}

  
}
