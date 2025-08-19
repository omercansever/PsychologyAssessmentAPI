
// Services/IAssessmentService.cs
using PsychologyAssessmentAPI.Models.DTOs;

namespace PsychologyAssessmentAPI.Services
{
    public interface IAssessmentService
    {
        Task<AssessmentResultDto> CalculateAssessmentAsync(SubmitAnswersDto submitAnswersDto);
        Task<GetQuestionsResponseDto> GetAllQuestionsAsync();
    }
}