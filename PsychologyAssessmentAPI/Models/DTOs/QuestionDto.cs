namespace PsychologyAssessmentAPI.Models.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Weight { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateQuestionDto
    {
        public string Text { get; set; }
        public int Weight { get; set; } = 1;
        public int CategoryId { get; set; }
    }

    public class QuestionForAssessmentDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string CategoryName { get; set; }
    }
}
