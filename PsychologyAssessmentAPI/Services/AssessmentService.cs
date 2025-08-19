using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Models.DTOs;

namespace PsychologyAssessmentAPI.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AssessmentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GetQuestionsResponseDto> GetAllQuestionsAsync()
        {
            var questions = await _context.Questions
                .Include(q => q.Category)
                .Where(q => q.IsActive)
                .OrderBy(q => q.CategoryId)
                .ThenBy(q => q.Id)
                .ToListAsync();

            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new GetQuestionsResponseDto
            {
                Questions = _mapper.Map<List<QuestionForAssessmentDto>>(questions),
                Categories = _mapper.Map<List<CategoryDto>>(categories)
            };
        }

        public async Task<AssessmentResultDto> CalculateAssessmentAsync(SubmitAnswersDto submitAnswersDto)
        {
            // Soruları ve kategorileri getir
            var questions = await _context.Questions
                .Include(q => q.Category)
                .Where(q => submitAnswersDto.Answers.Select(a => a.QuestionId).Contains(q.Id))
                .ToListAsync();

            // Kategorilere göre grupla ve hesapla
            var categoryResults = new List<CategoryResultDto>();
            
            var categoriesWithAnswers = questions
                .GroupBy(q => q.Category)
                .ToList();

            foreach (var categoryGroup in categoriesWithAnswers)
            {
                var category = categoryGroup.Key;
                var categoryQuestions = categoryGroup.ToList();
                
                // Bu kategorideki cevapları al
                var categoryAnswers = submitAnswersDto.Answers
                    .Where(a => categoryQuestions.Select(q => q.Id).Contains(a.QuestionId))
                    .ToList();

                if (categoryAnswers.Count == 0) continue;

                // Basit ortalama hesapla
                var simpleAverage = categoryAnswers.Average(a => a.AnswerValue);

                // Ağırlıklı ortalama hesapla
                var weightedSum = categoryAnswers.Sum(a => 
                {
                    var question = categoryQuestions.First(q => q.Id == a.QuestionId);
                    return a.AnswerValue * question.Weight;
                });
                var totalWeight = categoryAnswers.Sum(a => 
                {
                    var question = categoryQuestions.First(q => q.Id == a.QuestionId);
                    return question.Weight;
                });
                var weightedAverage = (decimal)weightedSum / totalWeight;

                var categoryResult = new CategoryResultDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Score = Math.Round((decimal)simpleAverage, 2),
                    WeightedScore = Math.Round(weightedAverage, 2),
                    ScoreLevel = GetScoreLevel(weightedAverage),
                    Recommendation = GetRecommendation(category.Name, weightedAverage)
                };

                categoryResults.Add(categoryResult);
            }

            // Genel değerlendirme
            var overallScore = categoryResults.Average(cr => cr.WeightedScore);
            var overallAssessment = GetOverallAssessment(categoryResults, overallScore);

            return new AssessmentResultDto
            {
                CategoryResults = categoryResults,
                OverallAssessment = overallAssessment,
                CalculatedAt = DateTime.UtcNow
            };
        }

        private string GetScoreLevel(decimal score)
        {
            return score switch
            {
                >= 4.0m => "Yüksek",
                >= 2.5m => "Orta",
                _ => "Düşük"
            };
        }

        private string GetRecommendation(string categoryName, decimal score)
        {
            var level = GetScoreLevel(score);
            
            return categoryName.ToLower() switch
            {
                "anksiyete" => level switch
                {
                    "Yüksek" => "Anksiyete seviyeniz yüksek görünüyor. Nefes egzersizleri, meditasyon ve gevşeme teknikleri deneyebilirsiniz. Profesyonel destek almanızı öneririz.",
                    "Orta" => "Orta seviyede anksiyete belirtileri gösteriyorsunuz. Düzenli egzersiz, yeterli uyku ve stres yönetimi tekniklerine odaklanın.",
                    _ => "Anksiyete seviyeniz normal aralıkta. Mevcut yaşam tarzınızı sürdürmeye devam edebilirsiniz."
                },
                "depresyon" => level switch
                {
                    "Yüksek" => "Depresif belirtiler gösteriyorsunuz. Lütfen bir ruh sağlığı uzmanından yardım alın. Sosyal destek almaya ve günlük aktivitelerinizi sürdürmeye odaklanın.",
                    "Orta" => "Bazı depresif belirtiler gösteriyorsunuz. Sosyal aktivitelerinizi artırın, düzenli egzersiz yapın ve hobi edinin.",
                    _ => "Ruh haliniz iyi durumda. Pozitif yaşam tarzınızı sürdürün."
                },
                "stres" => level switch
                {
                    "Yüksek" => "Yüksek stres seviyesi yaşıyorsunuz. Stres yönetimi teknikleri öğrenin, iş-yaşam dengesini sağlayın ve gerekirse profesyonel destek alın.",
                    "Orta" => "Orta seviyede stres yaşıyorsunuz. Zaman yönetimi, öncelik belirleme ve gevşeme aktivitelerine zaman ayırın.",
                    _ => "Stres seviyeniz kontrol altında. Mevcut stres yönetimi stratejilerinizi sürdürün."
                },
                "uyku" => level switch
                {
                    "Düşük" => "Uyku kaliteniz düşük görünüyor. Uyku hijyeni kurallarına uyun, düzenli uyku saatleri belirleyin ve yatak odası ortamını iyileştirin.",
                    "Orta" => "Uyku kaliteniz orta seviyede. Uyku rutininizi iyileştirmek için çaba gösterin.",
                    _ => "Uyku kaliteniz iyi durumda. Mevcut uyku alışkanlıklarınızı sürdürün."
                },
                "sosyal ilişkiler" => level switch
                {
                    "Düşük" => "Sosyal ilişkilerinizde zorluklar yaşıyor olabilirsiniz. Sosyal aktivitelere katılmayı deneyin ve iletişim becerileri geliştirin.",
                    "Orta" => "Sosyal ilişkileriniz orta seviyede. Mevcut ilişkilerinizi güçlendirmeye odaklanın.",
                    _ => "Sosyal ilişkileriniz sağlıklı görünüyor. Bu pozitif durumu sürdürün."
                },
                _ => "Bu kategori için özel önerilerimiz bulunmuyor. Genel olarak dengeli bir yaşam tarzı sürdürmeye odaklanın."
            };
        }

        private string GetOverallAssessment(List<CategoryResultDto> categoryResults, decimal overallScore)
        {
            var highRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Yüksek").ToList();
            var moderateRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Orta").ToList();

            if (highRiskCategories.Count >= 2)
            {
                return $"Birden fazla alanda yüksek seviye belirtiler gösteriyorsunuz ({string.Join(", ", highRiskCategories.Select(c => c.CategoryName))}). Profesyonel destek almanızı önemle tavsiye ederiz.";
            }
            else if (highRiskCategories.Count == 1)
            {
                return $"{highRiskCategories.First().CategoryName} alanında yüksek seviye belirtiler gösteriyorsunuz. Bu konuda özel olarak destek almanızı öneririz.";
            }
            else if (moderateRiskCategories.Count >= 3)
            {
                return "Birçok alanda orta seviye belirtiler gösteriyorsunuz. Genel yaşam kalitenizi artırmak için önerilerimizi dikkate alın.";
            }
            else if (overallScore >= 3.5m)
            {
                return "Genel olarak iyi bir ruh sağlığı profiline sahipsiniz. Mevcut pozitif alışkanlıklarınızı sürdürün.";
            }
            else
            {
                return "Ruh sağlığınız genel olarak normal aralıkta görünüyor. Önerilen iyileştirmeleri hayata geçirerek daha da iyi hissedebilirsiniz.";
            }
        }
    }
}