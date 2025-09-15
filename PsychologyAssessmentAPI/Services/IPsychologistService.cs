// Services/IPsychologistService.cs
using PsychologyAssessmentAPI.Models;
using PsychologyAssessmentAPI.Models.DTOs;

namespace PsychologyAssessmentAPI.Services
{
    public interface IPsychologistService
    {
        Task<GetPsychologistsResponseDto> GetAllPsychologistsAsync(PsychologistFilterDto filter);
        Task<PsychologistDto> GetPsychologistByIdAsync(int id);
        Task<PsychologistDto> CreatePsychologistAsync(CreatePsychologistDto createDto);
        Task<PsychologistDto> UpdatePsychologistAsync(int id, UpdatePsychologistDto updateDto);
        Task<bool> DeletePsychologistAsync(int id);
        Task<List<string>> GetSpecializationsAsync();
        Task<List<PsychologistDto>> GetNearbyPsychologistsAsync(double latitude, double longitude, double radiusKm);
    }
}