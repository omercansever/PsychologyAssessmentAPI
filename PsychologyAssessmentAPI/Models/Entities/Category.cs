using System.ComponentModel.DataAnnotations;

namespace PsychologyAssessmentAPI.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}