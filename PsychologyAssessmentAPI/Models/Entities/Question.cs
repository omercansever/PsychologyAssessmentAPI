using System.ComponentModel.DataAnnotations;

namespace PsychologyAssessmentAPI.Models.Entities
{
    public class Question
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }
        
        public int Weight { get; set; } = 1; // Sorunun ağırlığı
        
        public int CategoryId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Category Category { get; set; }
    }
}