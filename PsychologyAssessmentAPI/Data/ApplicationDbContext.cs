// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Models.Entities;

namespace PsychologyAssessmentAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category configurations
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Question configurations
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Text).IsRequired().HasMaxLength(1000);
                entity.Property(q => q.Weight).HasDefaultValue(1);
                
                entity.HasOne(q => q.Category)
                      .WithMany(c => c.Questions)
                      .HasForeignKey(q => q.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Örnek kategoriler
            modelBuilder.Entity<Category>(entity =>
{
    entity.HasKey(c => c.Id);
    entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
    entity.Property(c => c.Description).HasMaxLength(500);
    entity.HasIndex(c => c.Name).IsUnique();

    // ID'ler migration ile uyumlu olsun
    entity.Property(c => c.Id).ValueGeneratedNever();
});

modelBuilder.Entity<Question>(entity =>
{
    entity.HasKey(q => q.Id);
    entity.Property(q => q.Text).IsRequired().HasMaxLength(1000);
    entity.Property(q => q.Weight).HasDefaultValue(1);

    entity.HasOne(q => q.Category)
          .WithMany(c => c.Questions)
          .HasForeignKey(q => q.CategoryId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.Property(q => q.Id).ValueGeneratedNever();
});
        }
    }
}