// Controllers/AssessmentController.cs
using Microsoft.AspNetCore.Mvc;
using PsychologyAssessmentAPI.Models.DTOs;
using PsychologyAssessmentAPI.Services;

namespace PsychologyAssessmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        [HttpGet("questions")]
        public async Task<ActionResult<GetQuestionsResponseDto>> GetAllQuestions()
        {
            try
            {
                var result = await _assessmentService.GetAllQuestionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Sorular getirilirken hata oluştu: {ex.Message}");
            }
        }

        [HttpPost("submit")]
        public async Task<ActionResult<AssessmentResultDto>> SubmitAnswers(SubmitAnswersDto submitAnswersDto)
        {
            try
            {
                // Cevapları doğrula
                if (submitAnswersDto.Answers == null || submitAnswersDto.Answers.Count == 0)
                {
                    return BadRequest("Cevaplar boş olamaz.");
                }

                // Cevap değerlerini kontrol et (1-5 arası)
                var invalidAnswers = submitAnswersDto.Answers
                    .Where(a => a.AnswerValue < 1 || a.AnswerValue > 5)
                    .ToList();

                if (invalidAnswers.Any())
                {
                    return BadRequest("Cevap değerleri 1-5 arasında olmalıdır.");
                }

                // Aynı soruya birden fazla cevap var mı kontrol et
                var duplicateQuestions = submitAnswersDto.Answers
                    .GroupBy(a => a.QuestionId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateQuestions.Any())
                {
                    return BadRequest($"Aynı soruya birden fazla cevap verilmiş: {string.Join(", ", duplicateQuestions)}");
                }

                var result = await _assessmentService.CalculateAssessmentAsync(submitAnswersDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Değerlendirme hesaplanırken hata oluştu: {ex.Message}");
            }
        }

        [HttpPost("preview")]
        public async Task<ActionResult<AssessmentResultDto>> GetAssessmentPreview(SubmitAnswersDto submitAnswersDto)
        {
            try
            {
                if (submitAnswersDto.Answers == null || submitAnswersDto.Answers.Count == 0)
                {
                    return BadRequest("Cevaplar boş olamaz.");
                }

                var result = await _assessmentService.CalculateAssessmentAsync(submitAnswersDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Önizleme yapılırken hata oluştu: {ex.Message}");
            }
        }
    }
}
