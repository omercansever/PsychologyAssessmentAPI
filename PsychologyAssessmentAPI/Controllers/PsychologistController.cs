// Controllers/PsychologistController.cs
using Microsoft.AspNetCore.Mvc;
using PsychologyAssessmentAPI.Models.DTOs;
using PsychologyAssessmentAPI.Services;

namespace PsychologyAssessmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PsychologistController : ControllerBase
    {
        private readonly IPsychologistService _psychologistService;

        public PsychologistController(IPsychologistService psychologistService)
        {
            _psychologistService = psychologistService;
        }

        /// <summary>
        /// Tüm psikologları filtreli olarak getir
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GetPsychologistsResponseDto>> GetAllPsychologists([FromQuery] PsychologistFilterDto filter)
        {
            try
            {
                var result = await _psychologistService.GetAllPsychologistsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Psikologlar getirilirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// ID'ye göre psikolog getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PsychologistDto>> GetPsychologistById(int id)
        {
            try
            {
                var result = await _psychologistService.GetPsychologistByIdAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Psikolog getirilirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Yeni psikolog ekle
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PsychologistDto>> CreatePsychologist(CreatePsychologistDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _psychologistService.CreatePsychologistAsync(createDto);
                return CreatedAtAction(nameof(GetPsychologistById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Psikolog eklenirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Psikolog güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PsychologistDto>> UpdatePsychologist(int id, UpdatePsychologistDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _psychologistService.UpdatePsychologistAsync(id, updateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Psikolog güncellenirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Psikolog sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePsychologist(int id)
        {
            try
            {
                var result = await _psychologistService.DeletePsychologistAsync(id);
                
                if (!result)
                {
                    return NotFound($"ID {id} ile psikolog bulunamadı.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Psikolog silinirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm uzmanlık alanlarını getir
        /// </summary>
        [HttpGet("specializations")]
        public async Task<ActionResult<List<string>>> GetSpecializations()
        {
            try
            {
                var result = await _psychologistService.GetSpecializationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Uzmanlık alanları getirilirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir noktaya yakın psikologları getir
        /// </summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<List<PsychologistDto>>> GetNearbyPsychologists(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10)
        {
            try
            {
                if (latitude < -90 || latitude > 90)
                {
                    return BadRequest("Latitude -90 ile 90 arasında olmalıdır.");
                }

                if (longitude < -180 || longitude > 180)
                {
                    return BadRequest("Longitude -180 ile 180 arasında olmalıdır.");
                }

                if (radiusKm <= 0)
                {
                    return BadRequest("Yarıçap 0'dan büyük olmalıdır.");
                }

                var result = await _psychologistService.GetNearbyPsychologistsAsync(latitude, longitude, radiusKm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Yakındaki psikologlar getirilirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Psikolog istatistikleri
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var allPsychologists = await _psychologistService.GetAllPsychologistsAsync(new PsychologistFilterDto 
                { 
                    PageSize = int.MaxValue 
                });

                var statistics = new
                {
                    TotalCount = allPsychologists.TotalCount,
                    SpecializationCounts = allPsychologists.Psychologists
                        .GroupBy(p => p.Specialization)
                        .Select(g => new { Specialization = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    WithLocationCount = allPsychologists.Psychologists
                        .Count(p => p.Latitude.HasValue && p.Longitude.HasValue),
                    RecentlyAddedCount = allPsychologists.Psychologists
                        .Count(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"İstatistikler getirilirken hata oluştu: {ex.Message}");
            }
        }
    }
}