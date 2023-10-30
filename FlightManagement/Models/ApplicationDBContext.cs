using FlightManagement.Models.Management_Flight;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FlightManagement.Models
{
   public class ApplicationDBContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    {
        Addflights = Set<AddFlight>();
        DocumentInfo = Set<DocumentInformation>();
        Permiss = Set<Permission>();
    }

    public DbSet<AddFlight> Addflights { get; set; }
    public DbSet<DocumentInformation> DocumentInfo { get; set; }
    public DbSet<Permission> Permiss { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedRoles(builder);
        builder.Entity<AddFlight>()
        .Property(a => a.Date)
        .HasColumnType("date");

        builder.Entity<DocumentInformation>()
            .HasOne(d => d.Role)
            .WithMany()
            .HasForeignKey(d => d.RoleId);
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
            new IdentityRole { Name = "Employee", ConcurrencyStamp = "2", NormalizedName = "Employee" },
            new IdentityRole { Name = "Pilot", ConcurrencyStamp = "3", NormalizedName = "Pilot" },
            new IdentityRole { Name = "Crew", ConcurrencyStamp = "4", NormalizedName = "Crew" }
        );
    }
}

  
}
