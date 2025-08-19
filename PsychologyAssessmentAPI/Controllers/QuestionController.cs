// Controllers/QuestionsController.cs
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsychologyAssessmentAPI.Data;
using PsychologyAssessmentAPI.Models.DTOs;
using PsychologyAssessmentAPI.Models.Entities;

namespace PsychologyAssessmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public QuestionsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<QuestionDto>>> GetQuestions()
        {
            var questions = await _context.Questions
                .Include(q => q.Category)
                .Where(q => q.IsActive)
                .OrderBy(q => q.CategoryId)
                .ThenBy(q => q.Id)
                .ToListAsync();

            return Ok(_mapper.Map<List<QuestionDto>>(questions));
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<List<QuestionDto>>> GetQuestionsByCategory(int categoryId)
        {
            var questions = await _context.Questions
                .Include(q => q.Category)
                .Where(q => q.CategoryId == categoryId && q.IsActive)
                .OrderBy(q => q.Id)
                .ToListAsync();

            return Ok(_mapper.Map<List<QuestionDto>>(questions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound($"Soru bulunamadı. ID: {id}");
            }

            return Ok(_mapper.Map<QuestionDto>(question));
        }

        [HttpPost]
        public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionDto createQuestionDto)
        {
            // Kategori var mı kontrol et
            var category = await _context.Categories.FindAsync(createQuestionDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Geçersiz kategori ID.");
            }

            var question = _mapper.Map<Question>(createQuestionDto);
            
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Category bilgisi ile birlikte döndür
            await _context.Entry(question)
                .Reference(q => q.Category)
                .LoadAsync();

            var questionDto = _mapper.Map<QuestionDto>(question);
            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, questionDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, CreateQuestionDto updateQuestionDto)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound($"Soru bulunamadı. ID: {id}");
            }

            // Kategori var mı kontrol et
            var category = await _context.Categories.FindAsync(updateQuestionDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Geçersiz kategori ID.");
            }

            question.Text = updateQuestionDto.Text;
            question.Weight = updateQuestionDto.Weight;
            question.CategoryId = updateQuestionDto.CategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleQuestionActive(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound($"Soru bulunamadı. ID: {id}");
            }

            question.IsActive = !question.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { Id = id, IsActive = question.IsActive });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound($"Soru bulunamadı. ID: {id}");
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}