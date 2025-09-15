using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PsychologyAssessmentAPI.Models
{
    [Table("Psychologists")]
    public class Psychologist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(200)]
        public string Specialization { get; set; } // Dal/Uzmanlık alanı

        // YENİ: Profesyonel türü (Psikolog/Psikiyatrist)
        [Required]
        public ProfessionalType ProfessionalType { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        // Koordinat bilgileri (harita için)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
    }

    // YENİ: Profesyonel türü enum'u
    public enum ProfessionalType
    {
        [Display(Name = "Psikolog")]
        Psikolog = 1,
        
        [Display(Name = "Psikiyatrist")]
        Psikiyatrist = 2
    }
}