using Baigiamasis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Baigiamasis
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<HumanInformation> HumanInformation { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Roles)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // Configure User entity
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure one-to-one relationship between User and HumanInformation
            modelBuilder.Entity<User>()
                .HasOne(u => u.HumanInformation)
                .WithOne(h => h.User)
                .HasForeignKey<HumanInformation>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-one relationship between HumanInformation and Address
            modelBuilder.Entity<HumanInformation>()
                .HasOne(h => h.Address)
                .WithOne(a => a.HumanInformation)
                .HasForeignKey<Address>(a => a.HumanInformationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}