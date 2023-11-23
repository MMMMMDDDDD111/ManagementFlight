using FlightManagement.Models.Authentication.Login;
using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
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
   public DbSet<UpdateVersion> UpdateVersions { get; set; }
   public DbSet<PreviousVersion> PreviousVersions { get; set; }


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

         builder.Entity<DocumentInformation>()
            .HasOne(d => d.AddFlight)
            .WithMany(af => af.DocumentInformation)
            .HasForeignKey(d => d.IdFlight)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UpdateVersion>()
              .HasMany(uv => uv.PreviousVersions)
              .WithOne(pv => pv.UpdateVersion)
              .HasForeignKey(pv => pv.UpdateVersionId);

            builder.Entity<DocumentInformation>()
              .HasMany(d => d.UpdateVersions)
              .WithOne(u => u.DocumentInformation)
              .HasForeignKey(u => u.DocID)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UpdateVersion>()
                .HasMany(u => u.PreviousVersions)
                .WithOne(p => p.UpdateVersion)
                .HasForeignKey(p => p.UpdateVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Groups>(entity =>
            {
                entity.ToTable("Group");
                entity.HasKey(g => g.GroupId);
                entity.Property(g => g.GroupName)
                    .IsRequired()
                    .HasAnnotation("RegularExpression", "^(pilot|crew)$")
                    .HasAnnotation("ErrorMessage", "GroupName must be 'pilot' or 'crew");
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
