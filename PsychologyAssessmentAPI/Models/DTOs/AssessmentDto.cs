using System.ComponentModel.DataAnnotations;

namespace PsychologyAssessmentAPI.Models.DTOs
{
    public class SubmitAnswersDto
    {
        [Required]
        public List<AnswerDto> Answers { get; set; }
        
        // Kullanıcı konumu
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
        
        // Yakındaki uzmanlar için yarıçap (km) - varsayılan 50km
        public double RadiusKm { get; set; } = 50;
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public int AnswerValue { get; set; } // 1-5 arası
    }

    public class AssessmentResultDto
    {
        public List<CategoryResultDto> CategoryResults { get; set; }
        public string OverallAssessment { get; set; }
        
        // YENİ: Önerilen profesyonel türü
        public ProfessionalType RecommendedProfessionalType { get; set; }
        public string RecommendedProfessionalTypeText => RecommendedProfessionalType == ProfessionalType.Psikolog ? "Psikolog" : "Psikiyatrist";
        
        // YENİ: Kararın açıklaması
        public string ProfessionalRecommendationReason { get; set; }
        
        // YENİ: Yakındaki önerilen uzmanlar (kullanıcı konumu varsa)
        public List<PsychologistDto> NearbyRecommendedProfessionals { get; set; }
        
        public DateTime CalculatedAt { get; set; }
    }

    public class CategoryResultDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Score { get; set; }
        public decimal WeightedScore { get; set; }
        public string ScoreLevel { get; set; }
        public string Recommendation { get; set; }
    }

    // Güncellenmiş GetQuestionsResponseDto ve diğerleri...
    public class GetQuestionsResponseDto
    {
        public List<QuestionForAssessmentDto> Questions { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }

    public class QuestionForAssessmentDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Weight { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}