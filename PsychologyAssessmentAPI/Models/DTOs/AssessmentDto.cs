namespace PsychologyAssessmentAPI.Models.DTOs
{
    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public int AnswerValue { get; set; } // 1-5 arası likert ölçeği
    }

    public class SubmitAnswersDto
    {
        public List<AnswerDto> Answers { get; set; }
    }

    public class CategoryResultDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Score { get; set; } // Kategorideki ortalama puan
        public decimal WeightedScore { get; set; } // Ağırlıklı ortalama puan
        public string ScoreLevel { get; set; } // "Düşük", "Orta", "Yüksek"
        public string Recommendation { get; set; } // Öneriler
    }

    public class AssessmentResultDto
    {
        public List<CategoryResultDto> CategoryResults { get; set; }
        public string OverallAssessment { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class GetQuestionsResponseDto
    {
        public List<QuestionForAssessmentDto> Questions { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}