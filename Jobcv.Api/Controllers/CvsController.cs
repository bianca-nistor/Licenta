using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CvsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CvsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCv(CreateCvDto dto)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == dto.UserId);
            if (!userExists)
            {
                return NotFound("Utilizatorul nu există.");
            }

            var cv = new Cv
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Language = dto.Language,
                Summary = dto.Summary
            };

            _context.Cvs.Add(cv);
            await _context.SaveChangesAsync();

            return Ok(cv);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCvs(int userId)
        {
            var cvs = await _context.Cvs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(cvs);
        }

        [HttpGet("{cvId}")]
        public async Task<IActionResult> GetCvById(int cvId)
        {
            var cv = await _context.Cvs
                .Include(x => x.Skills)
                .Include(x => x.Experiences)
                .Include(x => x.Educations)
                .FirstOrDefaultAsync(x => x.Id == cvId);

            if (cv == null)
            {
                return NotFound("CV-ul nu a fost găsit.");
            }

            return Ok(cv);
        }
        [HttpPut("{cvId}")]
        public async Task<IActionResult> UpdateCv(int cvId, UpdateCvDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
            {
                return NotFound("CV-ul nu a fost găsit.");
            }

            cv.Title = dto.Title;
            cv.Language = dto.Language;
            cv.Summary = dto.Summary;

            await _context.SaveChangesAsync();

            return Ok(cv);
        }

        [HttpDelete("{cvId}")]
        public async Task<IActionResult> DeleteCv(int cvId)
        {
            var cv = await _context.Cvs.FindAsync(cvId);

            if (cv == null)
            {
                return NotFound("CV-ul nu a fost găsit.");
            }

            _context.Cvs.Remove(cv);
            await _context.SaveChangesAsync();

            return Ok("CV-ul a fost șters cu succes.");
        }

        [HttpDelete("skills/{skillId}")]
        public async Task<IActionResult> DeleteSkill(int skillId)
        {
            var skill = await _context.CvSkills.FindAsync(skillId);

            if (skill == null)
            {
                return NotFound("Skill-ul nu a fost găsit.");
            }

            _context.CvSkills.Remove(skill);
            await _context.SaveChangesAsync();

            return Ok("Skill-ul a fost șters cu succes.");
        }

        [HttpDelete("educations/{educationId}")]
        public async Task<IActionResult> DeleteEducation(int educationId)
        {
            var education = await _context.CvEducations.FindAsync(educationId);

            if (education == null)
            {
                return NotFound("Educația nu a fost găsită.");
            }

            _context.CvEducations.Remove(education);
            await _context.SaveChangesAsync();

            return Ok("Educația a fost ștearsă cu succes.");
        }
        [HttpPut("skills/{skillId}")]
        public async Task<IActionResult> UpdateSkill(int skillId, UpdateSkillDto dto)
        {
            var skill = await _context.CvSkills.FindAsync(skillId);

            if (skill == null)
            {
                return NotFound("Skill-ul nu a fost găsit.");
            }

            skill.Name = dto.Name;

            await _context.SaveChangesAsync();

            return Ok(skill);
        }

        [HttpPut("educations/{educationId}")]
        public async Task<IActionResult> UpdateEducation(int educationId, UpdateEducationDto dto)
        {
            var education = await _context.CvEducations.FindAsync(educationId);

            if (education == null)
            {
                return NotFound("Educația nu a fost găsită.");
            }

            education.Institution = dto.Institution;
            education.Degree = dto.Degree;
            education.Description = dto.Description;
            education.StartDate = dto.StartDate;
            education.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();

            return Ok(education);
        }
        [HttpPut("experiences/{experienceId}")]
        public async Task<IActionResult> UpdateExperience(int experienceId, UpdateExperienceDto dto)
        {
            var experience = await _context.CvExperiences.FindAsync(experienceId);

            if (experience == null)
            {
                return NotFound("Experiența nu a fost găsită.");
            }

            experience.JobTitle = dto.JobTitle;
            experience.Company = dto.Company;
            experience.Description = dto.Description;
            experience.StartDate = dto.StartDate;
            experience.EndDate = dto.EndDate;
            experience.IsCurrent = dto.IsCurrent;

            await _context.SaveChangesAsync();

            return Ok(experience);
        }

        [HttpDelete("experiences/{experienceId}")]
        public async Task<IActionResult> DeleteExperience(int experienceId)
        {
            var experience = await _context.CvExperiences.FindAsync(experienceId);

            if (experience == null)
            {
                return NotFound("Experiența nu a fost găsită.");
            }

            _context.CvExperiences.Remove(experience);
            await _context.SaveChangesAsync();

            return Ok("Experiența a fost ștearsă cu succes.");
        }

        [HttpPost("{cvId}/skills")]

        public async Task<IActionResult> AddSkill(int cvId, AddSkillDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);
            if (cv == null)
            {
                return NotFound("CV-ul nu există.");
            }

            var skill = new CvSkill
            {
                CvId = cvId,
                Name = dto.Name
            };

            _context.CvSkills.Add(skill);
            await _context.SaveChangesAsync();

            return Ok(skill);
        }

        [HttpPost("{cvId}/experiences")]
        public async Task<IActionResult> AddExperience(int cvId, AddExperienceDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);
            if (cv == null)
            {
                return NotFound("CV-ul nu există.");
            }

            var experience = new CvExperience
            {
                CvId = cvId,
                JobTitle = dto.JobTitle,
                Company = dto.Company,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsCurrent = dto.IsCurrent
            };

            _context.CvExperiences.Add(experience);
            await _context.SaveChangesAsync();

            return Ok(experience);
        }

        [HttpPost("{cvId}/educations")]
        public async Task<IActionResult> AddEducation(int cvId, AddEducationDto dto)
        {
            var cv = await _context.Cvs.FindAsync(cvId);
            if (cv == null)
            {
                return NotFound("CV-ul nu există.");
            }

            var education = new CvEducation
            {
                CvId = cvId,
                Institution = dto.Institution,
                Degree = dto.Degree,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _context.CvEducations.Add(education);
            await _context.SaveChangesAsync();

            return Ok(education);
        }

    }
}