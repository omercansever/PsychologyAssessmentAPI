using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Models;
using PsychologyAssessmentAPI.Models.DTOs;

namespace PsychologyAssessmentAPI.Services
{
    public class PsychologistService : IPsychologistService
    {
        private readonly ApplicationDbContext _context;

        public PsychologistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetPsychologistsResponseDto> GetAllPsychologistsAsync(PsychologistFilterDto filter)
        {
            var query = _context.Psychologists.AsQueryable();

            // Filtreleme
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(searchTerm) ||
                    p.LastName.ToLower().Contains(searchTerm) ||
                    p.Specialization.ToLower().Contains(searchTerm) ||
                    p.Address.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(filter.Specialization))
            {
                query = query.Where(p => p.Specialization.ToLower().Contains(filter.Specialization.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(p => p.Address.ToLower().Contains(filter.City.ToLower()));
            }

            // YENİ: Profesyonel türü filtresi
            if (filter.ProfessionalType.HasValue)
            {
                query = query.Where(p => p.ProfessionalType == filter.ProfessionalType.Value);
            }

            // Yakınlık filtresi
            if (filter.Latitude.HasValue && filter.Longitude.HasValue && filter.RadiusKm.HasValue)
            {
                query = query.Where(p => p.Latitude.HasValue && p.Longitude.HasValue);
                var psychologists = await query.ToListAsync();

                var nearbyPsychologists = psychologists
                    .Select(p => new
                    {
                        Professional = p,
                        Distance = CalculateDistance(filter.Latitude.Value, filter.Longitude.Value,
                                                   p.Latitude.Value, p.Longitude.Value)
                    })
                    .Where(p => p.Distance <= filter.RadiusKm.Value)
                    .OrderBy(p => p.Distance)
                    .ToList();

                var totalCount = nearbyPsychologists.Count;
                var pagedNearby = nearbyPsychologists
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => MapToPsychologistDto(p.Professional, p.Distance))
                    .ToList();

                return new GetPsychologistsResponseDto
                {
                    Psychologists = pagedNearby,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    HasNextPage = filter.PageNumber * filter.PageSize < totalCount,
                    HasPreviousPage = filter.PageNumber > 1
                };
            }

            // Sıralama
            query = filter.SortBy.ToLower() switch
            {
                "specialization" => filter.SortDescending ?
                    query.OrderByDescending(p => p.Specialization) :
                    query.OrderBy(p => p.Specialization),
                "professionaltype" => filter.SortDescending ?
                    query.OrderByDescending(p => p.ProfessionalType) :
                    query.OrderBy(p => p.ProfessionalType),
                "createdat" => filter.SortDescending ?
                    query.OrderByDescending(p => p.CreatedAt) :
                    query.OrderBy(p => p.CreatedAt),
                _ => filter.SortDescending ?
                    query.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName) :
                    query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            };

            var total = await query.CountAsync();
            var psychologistList = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new GetPsychologistsResponseDto
            {
                Psychologists = psychologistList.Select(p => MapToPsychologistDto(p)).ToList(),
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                HasNextPage = filter.PageNumber * filter.PageSize < total,
                HasPreviousPage = filter.PageNumber > 1
            };
        }

        public async Task<PsychologistDto> GetPsychologistByIdAsync(int id)
        {
            var psychologist = await _context.Psychologists.FindAsync(id);

            if (psychologist == null)
                throw new ArgumentException($"ID {id} ile uzman bulunamadı.");

            return MapToPsychologistDto(psychologist);
        }

        public async Task<PsychologistDto> CreatePsychologistAsync(CreatePsychologistDto createDto)
        {
            // E-posta kontrolü
            var existingPsychologist = await _context.Psychologists
                .FirstOrDefaultAsync(p => p.Email.ToLower() == createDto.Email.ToLower());

            if (existingPsychologist != null)
                throw new ArgumentException("Bu e-posta adresi ile kayıtlı bir uzman zaten mevcut.");

            var psychologist = new Psychologist
            {
                FirstName = createDto.FirstName.Trim(),
                LastName = createDto.LastName.Trim(),
                Specialization = createDto.Specialization.Trim(),
                ProfessionalType = createDto.ProfessionalType, // YENİ ALAN
                PhoneNumber = createDto.PhoneNumber.Trim(),
                Email = createDto.Email.Trim().ToLower(),
                Address = createDto.Address.Trim(),
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.Psychologists.Add(psychologist);
            await _context.SaveChangesAsync();

            return MapToPsychologistDto(psychologist);
        }

        public async Task<PsychologistDto> UpdatePsychologistAsync(int id, UpdatePsychologistDto updateDto)
        {
            var psychologist = await _context.Psychologists.FindAsync(id);

            if (psychologist == null)
                throw new ArgumentException($"ID {id} ile uzman bulunamadı.");

            // E-posta kontrolü (kendisi hariç)
            var existingPsychologist = await _context.Psychologists
                .FirstOrDefaultAsync(p => p.Email.ToLower() == updateDto.Email.ToLower() && p.Id != id);

            if (existingPsychologist != null)
                throw new ArgumentException("Bu e-posta adresi ile kayıtlı başka bir uzman mevcut.");

            // Güncelleme
            psychologist.FirstName = updateDto.FirstName.Trim();
            psychologist.LastName = updateDto.LastName.Trim();
            psychologist.Specialization = updateDto.Specialization.Trim();
            psychologist.ProfessionalType = updateDto.ProfessionalType; // YENİ ALAN
            psychologist.PhoneNumber = updateDto.PhoneNumber.Trim();
            psychologist.Email = updateDto.Email.Trim().ToLower();
            psychologist.Address = updateDto.Address.Trim();
            psychologist.Latitude = updateDto.Latitude;
            psychologist.Longitude = updateDto.Longitude;
            psychologist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPsychologistDto(psychologist);
        }

        public async Task<bool> DeletePsychologistAsync(int id)
        {
            var psychologist = await _context.Psychologists.FindAsync(id);

            if (psychologist == null)
                return false;

            _context.Psychologists.Remove(psychologist);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<string>> GetSpecializationsAsync()
        {
            return await _context.Psychologists
                .Select(p => p.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        // YENİ: Profesyonel türüne göre uzmanlık alanlarını getir
        public async Task<List<string>> GetSpecializationsByTypeAsync(ProfessionalType professionalType)
        {
            return await _context.Psychologists
                .Where(p => p.ProfessionalType == professionalType)
                .Select(p => p.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<List<PsychologistDto>> GetNearbyPsychologistsAsync(double latitude, double longitude, double radiusKm)
        {
            var psychologists = await _context.Psychologists
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
                .ToListAsync();

            var nearbyPsychologists = psychologists
                .Select(p => new
                {
                    Professional = p,
                    Distance = CalculateDistance(latitude, longitude, p.Latitude.Value, p.Longitude.Value)
                })
                .Where(p => p.Distance <= radiusKm)
                .OrderBy(p => p.Distance)
                .Select(p => MapToPsychologistDto(p.Professional, p.Distance))
                .ToList();

            return nearbyPsychologists;
        }

        // YENİ: Belirli türdeki yakındaki uzmanları getir
        public async Task<List<PsychologistDto>> GetNearbyProfessionalsByTypeAsync(
            ProfessionalType professionalType,
            double latitude,
            double longitude,
            double radiusKm)
        {
            var professionals = await _context.Psychologists
                .Where(p => p.ProfessionalType == professionalType &&
                           p.Latitude.HasValue &&
                           p.Longitude.HasValue)
                .ToListAsync();

            var nearbyProfessionals = professionals
                .Select(p => new
                {
                    Professional = p,
                    Distance = CalculateDistance(latitude, longitude, p.Latitude.Value, p.Longitude.Value)
                })
                .Where(p => p.Distance <= radiusKm)
                .OrderBy(p => p.Distance)
                .Select(p => MapToPsychologistDto(p.Professional, p.Distance))
                .ToList();

            return nearbyProfessionals;
        }

        private static PsychologistDto MapToPsychologistDto(Psychologist psychologist, double? distance = null)
        {
            return new PsychologistDto
            {
                Id = psychologist.Id,
                FirstName = psychologist.FirstName,
                LastName = psychologist.LastName,
                Specialization = psychologist.Specialization,
                ProfessionalType = psychologist.ProfessionalType, // YENİ ALAN
                PhoneNumber = psychologist.PhoneNumber,
                Email = psychologist.Email,
                Address = psychologist.Address,
                Latitude = psychologist.Latitude,
                Longitude = psychologist.Longitude,
                DistanceKm = distance.HasValue ? Math.Round(distance.Value, 1) : null, // YENİ ALAN
                CreatedAt = psychologist.CreatedAt,
                UpdatedAt = psychologist.UpdatedAt
            };
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
    }
}