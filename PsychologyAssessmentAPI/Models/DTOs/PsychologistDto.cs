using System.ComponentModel.DataAnnotations;

namespace PsychologyAssessmentAPI.Models.DTOs
{
    public class PsychologistDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Specialization { get; set; }
        
        // YENİ: Profesyonel türü
        public ProfessionalType ProfessionalType { get; set; }
        public string ProfessionalTypeText => ProfessionalType == ProfessionalType.Psikolog ? "Psikolog" : "Psikiyatrist";
        
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        // YENİ: Kullanıcıya olan mesafe (hesaplanırsa)
        public double? DistanceKm { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePsychologistDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad maksimum 100 karakter olabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Soyad maksimum 100 karakter olabilir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Uzmanlık alanı maksimum 200 karakter olabilir.")]
        public string Specialization { get; set; }

        // YENİ: Profesyonel türü
        [Required(ErrorMessage = "Profesyonel türü seçilmelidir.")]
        public ProfessionalType ProfessionalType { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "Telefon numarası maksimum 20 karakter olabilir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(255, ErrorMessage = "E-posta adresi maksimum 255 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        [StringLength(500, ErrorMessage = "Adres maksimum 500 karakter olabilir.")]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class UpdatePsychologistDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad maksimum 100 karakter olabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Soyad maksimum 100 karakter olabilir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Uzmanlık alanı maksimum 200 karakter olabilir.")]
        public string Specialization { get; set; }

        // YENİ: Profesyonel türü
        [Required(ErrorMessage = "Profesyonel türü seçilmelidir.")]
        public ProfessionalType ProfessionalType { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "Telefon numarası maksimum 20 karakter olabilir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(255, ErrorMessage = "E-posta adresi maksimum 255 karakter olabilir.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        [StringLength(500, ErrorMessage = "Adres maksimum 500 karakter olabilir.")]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class PsychologistFilterDto
    {
        public string SearchTerm { get; set; }
        public string Specialization { get; set; }
        public string City { get; set; }
        
        // YENİ: Profesyonel türü filtresi
        public ProfessionalType? ProfessionalType { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "FullName";
        public bool SortDescending { get; set; } = false;
    }

    public class GetPsychologistsResponseDto
    {
        public List<PsychologistDto> Psychologists { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}