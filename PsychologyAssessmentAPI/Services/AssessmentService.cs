using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Models;
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

            // Genel değerlendirme ve profesyonel türü belirleme
            var overallScore = categoryResults.Average(cr => cr.WeightedScore);
            var recommendedProfessionalType = DetermineProfessionalType(categoryResults, overallScore);
            var professionalRecommendationReason = GetProfessionalRecommendationReason(categoryResults, overallScore, recommendedProfessionalType);
            var overallAssessment = GetOverallAssessment(categoryResults, overallScore, recommendedProfessionalType);

            // Yakındaki uzmanları getir (konum bilgisi varsa)
            List<PsychologistDto> nearbyProfessionals = new List<PsychologistDto>();
            
            if (submitAnswersDto.UserLatitude.HasValue && submitAnswersDto.UserLongitude.HasValue)
            {
                nearbyProfessionals = await GetNearbyProfessionals(
                    recommendedProfessionalType,
                    submitAnswersDto.UserLatitude.Value,
                    submitAnswersDto.UserLongitude.Value,
                    submitAnswersDto.RadiusKm
                );
            }

            return new AssessmentResultDto
            {
                CategoryResults = categoryResults,
                OverallAssessment = overallAssessment,
                RecommendedProfessionalType = recommendedProfessionalType,
                ProfessionalRecommendationReason = professionalRecommendationReason,
                NearbyRecommendedProfessionals = nearbyProfessionals,
                CalculatedAt = DateTime.UtcNow
            };
        }

        #region Private Methods

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

        private ProfessionalType DetermineProfessionalType(List<CategoryResultDto> categoryResults, decimal overallScore)
        {
            // Yüksek risk kategorilerini say
            var highRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Yüksek").ToList();
            var moderateRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Orta").ToList();

            // Psikiyatriste yönlendirme kriterleri
            var severeMentalHealthCategories = new[] { "depresyon", "anksiyete", "panik atak", "obsesif kompulsif", "bipolar" };
            var hasSevereMentalHealthIssues = highRiskCategories
                .Any(cr => severeMentalHealthCategories.Any(severe => 
                    cr.CategoryName.ToLower().Contains(severe)));

            // Kararları
            if (hasSevereMentalHealthIssues && highRiskCategories.Count >= 2)
            {
                // Ciddi ruh sağlığı sorunu + birden fazla yüksek risk = Psikiyatrist
                return ProfessionalType.Psikiyatrist;
            }
            else if (highRiskCategories.Any(cr => cr.CategoryName.ToLower().Contains("depresyon") && cr.WeightedScore >= 4.5m))
            {
                // Çok yüksek depresyon skoru = Psikiyatrist
                return ProfessionalType.Psikiyatrist;
            }
            else if (overallScore >= 4.0m && highRiskCategories.Count >= 3)
            {
                // Genel skor çok yüksek + çok sayıda problem = Psikiyatrist
                return ProfessionalType.Psikiyatrist;
            }
            else
            {
                // Diğer durumlar = Psikolog
                return ProfessionalType.Psikolog;
            }
        }

        private string GetProfessionalRecommendationReason(List<CategoryResultDto> categoryResults, decimal overallScore, ProfessionalType recommendedType)
        {
            var highRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Yüksek").ToList();

            if (recommendedType == ProfessionalType.Psikiyatrist)
            {
                if (highRiskCategories.Any(cr => cr.CategoryName.ToLower().Contains("depresyon")))
                {
                    return "Depresyon belirtilerinizin yoğunluğu nedeniyle tıbbi değerlendirme ve gerekirse ilaç tedavisi için psikiyatrist öneriyoruz.";
                }
                else if (highRiskCategories.Count >= 2)
                {
                    return "Birden fazla alanda ciddi belirtiler gösterdiğiniz için kapsamlı tıbbi değerlendirme için psikiyatrist öneriyoruz.";
                }
                else
                {
                    return "Mevcut belirtilerinizin yoğunluğu tıbbi müdahale gerektirebilir, psikiyatrist ile görüşmenizi öneriyoruz.";
                }
            }
            else
            {
                return "Mevcut durumunuz terapi ve danışmanlık desteği ile iyileştirilebilir, psikolog ile görüşmenizi öneriyoruz.";
            }
        }

        private string GetOverallAssessment(List<CategoryResultDto> categoryResults, decimal overallScore, ProfessionalType recommendedType)
        {
            var highRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Yüksek").ToList();
            var moderateRiskCategories = categoryResults.Where(cr => cr.ScoreLevel == "Orta").ToList();

            var professionalText = recommendedType == ProfessionalType.Psikolog ? "psikolog" : "psikiyatrist";

            if (highRiskCategories.Count >= 2)
            {
                return $"Birden fazla alanda yüksek seviye belirtiler gösteriyorsunuz ({string.Join(", ", highRiskCategories.Select(c => c.CategoryName))}). {professionalText.Substring(0, 1).ToUpper() + professionalText.Substring(1)} desteği almanızı önemle tavsiye ederiz.";
            }
            else if (highRiskCategories.Count == 1)
            {
                return $"{highRiskCategories.First().CategoryName} alanında yüksek seviye belirtiler gösteriyorsunuz. Bu konuda {professionalText} desteği almanızı öneririz.";
            }
            else if (moderateRiskCategories.Count >= 3)
            {
                return $"Birçok alanda orta seviye belirtiler gösteriyorsunuz. Yaşam kalitenizi artırmak için {professionalText} desteği faydalı olacaktır.";
            }
            else if (overallScore >= 3.5m)
            {
                return "Genel olarak iyi bir ruh sağlığı profiline sahipsiniz. Mevcut pozitif alışkanlıklarınızı sürdürün.";
            }
            else
            {
                return $"Ruh sağlığınız genel olarak normal aralıkta görünüyor. Önerilen iyileştirmeler için {professionalText} desteği alabilirsiniz.";
            }
        }

        private async Task<List<PsychologistDto>> GetNearbyProfessionals(
            ProfessionalType professionalType,
            double userLatitude, 
            double userLongitude, 
            double radiusKm)
        {
            // Belirtilen türdeki uzmanları getir
            var professionals = await _context.Psychologists
                .Where(p => p.ProfessionalType == professionalType && 
                           p.Latitude.HasValue && 
                           p.Longitude.HasValue)
                .ToListAsync();

            // Mesafe hesapla ve filtrele
            var nearbyProfessionals = professionals
                .Select(p => new
                {
                    Professional = p,
                    Distance = CalculateDistance(userLatitude, userLongitude, p.Latitude.Value, p.Longitude.Value)
                })
                .Where(p => p.Distance <= radiusKm)
                .OrderBy(p => p.Distance)
                .Take(10) // En yakın 10 uzmanı al
                .Select(p => new PsychologistDto
                {
                    Id = p.Professional.Id,
                    FirstName = p.Professional.FirstName,
                    LastName = p.Professional.LastName,
                    Specialization = p.Professional.Specialization,
                    ProfessionalType = p.Professional.ProfessionalType,
                    PhoneNumber = p.Professional.PhoneNumber,
                    Email = p.Professional.Email,
                    Address = p.Professional.Address,
                    Latitude = p.Professional.Latitude,
                    Longitude = p.Professional.Longitude,
                    DistanceKm = Math.Round(p.Distance, 1),
                    CreatedAt = p.Professional.CreatedAt,
                    UpdatedAt = p.Professional.UpdatedAt
                })
                .ToList();

            return nearbyProfessionals;
        }

        // Haversine formülü ile mesafe hesaplama (km)
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Dünya'nın yarıçapı (km)

            var lat1Rad = lat1 * (Math.PI / 180);
            var lat2Rad = lat2 * (Math.PI / 180);
            var deltaLat = (lat2 - lat1) * (Math.PI / 180);
            var deltaLon = (lon2 - lon1) * (Math.PI / 180);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        #endregion
    }
}