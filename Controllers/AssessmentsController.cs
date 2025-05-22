using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncWebAPI.Data;
using EduSyncWebAPI.Models;
using EduSyncWebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssessmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AssessmentsController> _logger;

        public AssessmentsController(AppDbContext context, ILogger<AssessmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all assessments without questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
        {
            return await _context.Assessments
                .Select(a => new AssessmentDto
                {
                    AssessmentId = a.AssessmentId,
                    Title = a.Title,
                    MaxScore = a.MaxScore,
                    CourseId = a.CourseId
                }).ToListAsync();
        }

        // Get single assessment with questions and course info
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDetailDto>> GetAssessment(Guid id)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null) return NotFound();

            return new AssessmentDetailDto
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                MaxScore = assessment.MaxScore,
                Questions = assessment.Questions.Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Marks = q.Marks,
                    Options = q.Options.Select(o => new OptionDto
                    {
                        OptionId = o.OptionId,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList(),
                Course = assessment.Course == null ? null : new CourseReadDTO
                {
                    CourseId = assessment.Course.CourseId,
                    Title = assessment.Course.Title,
                    Description = assessment.Course.Description
                }
            };
        }

        // Create new assessment with questions
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<ActionResult<AssessmentDto>> PostAssessment(AssessmentCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.CourseId != null &&
                !await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId))
                return Conflict("Course with given ID does not exist.");

            var assessment = new Assessment
            {
                AssessmentId = Guid.NewGuid(),
                Title = dto.Title,
                MaxScore = dto.MaxScore,
                CourseId = dto.CourseId.Value,
                Questions = new List<Question>()
            };

            if (dto.Questions != null)
            {
                foreach (var qDto in dto.Questions)
                {
                    var question = new Question
                    {
                        QuestionId = Guid.NewGuid(),
                        Text = qDto.Text,
                        Marks = qDto.Marks,
                        Options = new List<Option>()
                    };

                    if (qDto.Options != null)
                    {
                        foreach (var oDto in qDto.Options)
                        {
                            question.Options.Add(new Option
                            {
                                OptionId = Guid.NewGuid(),
                                Text = oDto.Text,
                                IsCorrect = oDto.IsCorrect
                            });
                        }
                    }

                    assessment.Questions.Add(question);
                }
            }

            var resultDto = new AssessmentDto
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                MaxScore = assessment.MaxScore,
                CourseId = assessment.CourseId
            };

            return CreatedAtAction(nameof(GetAssessment), new { id = resultDto.AssessmentId }, resultDto);
        }

        // Update existing assessment and questions
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (id != dto.AssessmentId) return BadRequest("ID mismatch.");

            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null) return NotFound();

            if (dto.CourseId != null &&
                !await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId))
                return Conflict("Course with given ID does not exist.");

            assessment.Title = dto.Title;
            assessment.MaxScore = dto.MaxScore;
            assessment.CourseId = dto.CourseId.Value;

            // Remove existing questions & their options
            _context.Questions.RemoveRange(assessment.Questions);
            await _context.SaveChangesAsync();  // Save after removal to avoid tracking conflicts

            if (dto.Questions != null)
            {
                assessment.Questions = dto.Questions.Select(q => new Question
                {
                    QuestionId = Guid.NewGuid(),
                    Text = q.Text,
                    Marks = q.Marks,
                    Options = q.Options?.Select(o => new Option
                    {
                        OptionId = Guid.NewGuid(),
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList() ?? new List<Option>()
                }).ToList();
            }
            else
            {
                assessment.Questions = new List<Question>();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment");
                return StatusCode(500, "An error occurred while updating the assessment.");
            }

            return NoContent();
        }

        // Delete assessment with questions and options
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminOrInstructorRole")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessment == null) return NotFound();

            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Submit assessment answers and calculate score
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitAssessment([FromBody] AssessmentSubmissionDto submission)
        {
            if (submission == null || submission.Answers == null)
                return BadRequest("Invalid submission.");

            var assessment = await _context.Assessments
                .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssessmentId == submission.AssessmentId);

            if (assessment == null)
                return NotFound("Assessment not found");

            int score = 0;

            foreach (var answer in submission.Answers)
            {
                var question = assessment.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                if (question != null)
                {
                    var selectedOption = question.Options.FirstOrDefault(o => o.OptionId == answer.SelectedOptionId);
                    if (selectedOption != null && selectedOption.IsCorrect)
                    {
                        score += question.Marks;
                    }
                }
            }

            return Ok(new
            {
                Score = score,
                MaxScore = assessment.MaxScore
            });
        }

    }
}

