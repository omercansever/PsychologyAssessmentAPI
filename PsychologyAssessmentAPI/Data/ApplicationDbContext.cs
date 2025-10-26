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
            modelBuilder.Entity<Category>().HasData(
        new Category { Id = 1, Name = "GAB", Description = "Genel Anksiyete Bozukluğu soruları" },
        new Category { Id = 2, Name = "OKB", Description = "Obsesif Kompulsif Bozukluk soruları" },
        new Category { Id = 3, Name = "Bipolar", Description = "Bipolar Bozukluk soruları" },
        new Category { Id = 4, Name = "Depresyon", Description = "Depresyon soruları" },
        new Category { Id = 5, Name = "ADHD", Description = "Dikkat Eksikliği ve Hiperaktivite Bozukluğu soruları" },
        new Category { Id = 6, Name = "Şizofreni", Description = "Şizofreni soruları" },
        new Category { Id = 7, Name = "Sosyal Anksiyete", Description = "Sosyal Anksiyete Bozukluğu soruları" }
    );

    // ----- Sorular -----
    modelBuilder.Entity<Question>().HasData(
        // GAB
        new Question { Id = 1, Text = "Son 6 ay boyunca, çoğu gününde iş, okul ya da günlük olaylar hakkında aşırı kaygı ve kuruntu yaşadın mı?", Weight = 1, CategoryId = 1 },
        new Question { Id = 2, Text = "Son 6 ay boyunca, kendini çoğu gün huzursuz, gergin ya da sürekli diken üstünde hissedip günlük işlerini aksattığın oluyor mu?", Weight = 1, CategoryId = 1 },
        new Question { Id = 3, Text = "Son 6 ay boyunca, kaslarının gerginleştiğini ya da sürekli kaslarında bir sertlik hissettiğin oluyor mu?", Weight = 1, CategoryId = 1 },
        new Question { Id = 4, Text = "Son 6 ay boyunca, uykuya dalmakta ya da uykunu sürdürmekte zorlanıyor musun?", Weight = 1, CategoryId = 1 },

        // OKB
        new Question { Id = 5, Text = "Son birkaç ayda, zihninde tekrarlayan ve seni rahatsız eden imgeler ya da düşünceler ortaya çıkıyor mu?", Weight = 1, CategoryId = 2 },
        new Question { Id = 6, Text = "Son birkaç ayda, bu rahatsız edici düşünceleri başka bir eylemle (örneğin, bir şeyleri kontrol etme) yok etmeye çalıştığın oluyor mu?", Weight = 1, CategoryId = 2 },
        new Question { Id = 7, Text = "Son birkaç ayda, belirli kurallara uymak için tekrarlayan davranışlar (örneğin, eşyaları düzenleme, sayı sayma) yapma zorunluluğu hissettin mi?", Weight = 1, CategoryId = 2 },
        new Question { Id = 8, Text = "Son birkaç ayda, bu düşünceler ya da davranışlar yüzünden günlük işlerinde zorluk yaşadığın oluyor mu?", Weight = 1, CategoryId = 2 },

        // Bipolar
        new Question { Id = 9, Text = "Son bir hafta veya daha uzun süre her gün, kendini aşırı enerjik, coşkulu ya da çok konuşkan/hareketli hissedip riskli kararlar aldığın bir dönem yaşadın mı?", Weight = 1, CategoryId = 3 },
        new Question { Id = 10, Text = "Son bir hafta veya daha uzun süre boyunca, kendini olağandışı derecede önemli ya da üstün hissettiğin bir dönem yaşadın mı?", Weight = 1, CategoryId = 3 },
        new Question { Id = 11, Text = "Son bir hafta veya daha uzun süre boyunca, çok az uyuyarak kendini dinlenmiş hissettiğin bir dönem yaşadın mı?", Weight = 1, CategoryId = 3 },
        new Question { Id = 12, Text = "Son bir hafta boyunca, düşüncelerinin çok hızlı aktığını ya da birbiriyle yarışıyormuş gibi hissettiğin bir dönem yaşadın mı?", Weight = 1, CategoryId = 3 },

        // Depresyon
        new Question { Id = 13, Text = "Son iki hafta boyunca, kendinizi sürekli üzgün, umutsuz veya boşlukta hissettiniz mi?", Weight = 1, CategoryId = 4 },
        new Question { Id = 14, Text = "Son iki hafta boyunca, neredeyse bütün etkinliklere karşı ilgini kaybettiğini hissettin mi?", Weight = 1, CategoryId = 4 },
        new Question { Id = 15, Text = "Son iki hafta boyunca, uykusuzluk çektiğin ya da aşırı uyuduğun bir dönem yaşadın mı?", Weight = 1, CategoryId = 4 },
        new Question { Id = 16, Text = "Son iki hafta boyunca, diyet yapmıyorken kilo değişimi yaşadın mı?", Weight = 1, CategoryId = 4 },
        new Question { Id = 17, Text = "Son iki hafta boyunca, kendini değersiz hissettiğin veya aşırı suçlu hissettiğin bir dönem yaşadın mı?", Weight = 1, CategoryId = 4 },

        // ADHD
        new Question { Id = 18, Text = "Son 6 ay boyunca, başladığın bir işi sık sık yarım bırakıyor musun?", Weight = 1, CategoryId = 5 },
        new Question { Id = 19, Text = "Son 6 ay boyunca, bir işle uğraşırken dikkatin kolayca dağılıyor mu?", Weight = 1, CategoryId = 5 },
        new Question { Id = 20, Text = "Son 6 ay boyunca, aniden bir şey yapmaya karar verip aceleyle hata yaptığın oluyor mu?", Weight = 1, CategoryId = 5 },
        new Question { Id = 21, Text = "Son 6 ay boyunca, eşyalarını sık sık kaybediyor musun?", Weight = 1, CategoryId = 5 },

        // Şizofreni
        new Question { Id = 22, Text = "Son bir ay boyunca, neredeyse her gün, gerçek olmadığını bildiğiniz şeyleri duyuyor, görüyor veya hissediyor musunuz?", Weight = 1, CategoryId = 6 },
        new Question { Id = 23, Text = "Son bir ay boyunca, başkalarının sizi takip ettiği veya zarar vermeye çalıştığına dair güçlü bir inancınız var mı?", Weight = 1, CategoryId = 6 },
        new Question { Id = 24, Text = "Son bir ay boyunca, duygularınızı ifade etmekte zorlanıyor musunuz?", Weight = 1, CategoryId = 6 },
        new Question { Id = 25, Text = "Son bir ay boyunca, konuşmalarınızda konudan sapma veya anlaşılmaz ifadeler kullandığınızı fark ettiniz mi?", Weight = 1, CategoryId = 6 },

        // Sosyal Anksiyete
        new Question { Id = 26, Text = "Son altı ay veya daha uzun süre boyunca, toplumsal durumlarda belirgin bir kaygı hissettiniz mi?", Weight = 1, CategoryId = 7 },
        new Question { Id = 27, Text = "Sosyal ortamlardan kaçındığınız ya da bunlara katlanırken yoğun rahatsızlık duyduğunuz oluyor mu?", Weight = 1, CategoryId = 7 },
        new Question { Id = 28, Text = "Toplumsal durumlarda küçük düşme, utanç duyma ya da dışlanma korkusu yaşadığınız oluyor mu?", Weight = 1, CategoryId = 7 },
        new Question { Id = 29, Text = "Kaygı veya sosyal durumlardan kaçınma, günlük hayatınızda zorluklara neden oluyor mu?", Weight = 1, CategoryId = 7 }
    );
        }
    }
}