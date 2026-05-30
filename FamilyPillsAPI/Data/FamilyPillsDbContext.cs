using Microsoft.EntityFrameworkCore;
using FamilyPillsAPI.Models;

namespace FamilyPillsAPI.Data
{
    public class FamilyPillsDbContext : DbContext
    {
        public FamilyPillsDbContext(DbContextOptions<FamilyPillsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicine> Medicines { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Medicine entity
            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.TotalQuantity).IsRequired();
                entity.Property(e => e.IsRunningLow).HasDefaultValue(false);
                entity.Property(e => e.IsExpired).HasDefaultValue(false);
            });
        }
    }
}
