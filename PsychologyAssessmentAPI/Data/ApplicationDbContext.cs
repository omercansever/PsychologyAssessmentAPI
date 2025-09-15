// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Models;
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
        public DbSet<Psychologist> Psychologists { get; set; }

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
                
                // ID'ler migration ile uyumlu olsun
                entity.Property(c => c.Id).ValueGeneratedOnAdd();
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

                entity.Property(q => q.Id).ValueGeneratedOnAdd();
            });

            // Psychologist entity configuration
            modelBuilder.Entity<Psychologist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialization).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Email unique constraint
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Location için index
                entity.HasIndex(e => new { e.Latitude, e.Longitude });
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Örnek psikologlar
            modelBuilder.Entity<Psychologist>().HasData(
                new Psychologist
                {
                    Id = 1,
                    FirstName = "Dr. Mehmet",
                    LastName = "Yılmaz",
                    Specialization = "Klinik Psikoloji",
                    PhoneNumber = "+90 532 123 4567",
                    Email = "mehmet.yilmaz@example.com",
                    Address = "Serdivan, Sakarya",
                    Latitude = 40.7589,
                    Longitude = 30.3425,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Psychologist
                {
                    Id = 2,
                    FirstName = "Ayşe",
                    LastName = "Demir",
                    Specialization = "Çocuk Psikolojisi",
                    PhoneNumber = "+90 532 987 6543",
                    Email = "ayse.demir@example.com",
                    Address = "Adapazarı, Sakarya",
                    Latitude = 40.7808,
                    Longitude = 30.4034,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Psychologist
                {
                    Id = 3,
                    FirstName = "Prof. Dr. Fatma",
                    LastName = "Kaya",
                    Specialization = "Aile Danışmanlığı",
                    PhoneNumber = "+90 532 555 1234",
                    Email = "fatma.kaya@example.com",
                    Address = "Erenler, Sakarya",
                    Latitude = 40.7725,
                    Longitude = 30.3897,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}