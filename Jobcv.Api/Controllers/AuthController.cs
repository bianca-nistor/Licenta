using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobCv.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public AuthController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest("Toate câmpurile sunt obligatorii.");
            }

            var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);
            if (emailExists)
            {
                return BadRequest("Există deja un cont cu acest email.");
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
            {
                return Unauthorized("Email sau parolă incorectă.");
            }

            var validPassword = _passwordService.VerifyPassword(user.PasswordHash, dto.Password);
            if (!validPassword)
            {
                return Unauthorized("Email sau parolă incorectă.");
            }

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email
            });
        }
    }
}