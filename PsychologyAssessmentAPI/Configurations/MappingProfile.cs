// Configurations/MappingProfile.cs
using AutoMapper;
using PsychologyAssessmentAPI.Models.DTOs;
using PsychologyAssessmentAPI.Models.Entities;

namespace PsychologyAssessmentAPI.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();

            // Question mappings
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            
            CreateMap<Question, QuestionForAssessmentDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            
            CreateMap<CreateQuestionDto, Question>();
        }
    }
}