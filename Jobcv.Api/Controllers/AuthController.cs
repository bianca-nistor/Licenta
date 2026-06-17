using JobCv.Api.Data;
using JobCv.Api.Dtos;
using JobCv.Api.Models;
using JobCv.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

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
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("All fields are required.");
            }

            dto.Email = dto.Email.Trim().ToLower();

            if (!IsValidEmail(dto.Email))
            {
                return BadRequest("Please enter a valid email address.");
            }

            if (dto.Password.Length < 6)
            {
                return BadRequest("Password must have at least 6 characters.");
            }

            var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);
            if (emailExists)
            {
                return BadRequest("An account with this email already exists.");
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                PhoneNumber = string.Empty,
                AlternativeEmail = string.Empty,
                Location = string.Empty,
                CareerLevel = string.Empty,
                PreferredJobType = string.Empty
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ToUserResponse(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Email and password are required.");
            }

            dto.Email = dto.Email.Trim().ToLower();

            if (!IsValidEmail(dto.Email))
            {
                return BadRequest("Please enter a valid email address.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var validPassword = _passwordService.VerifyPassword(user.PasswordHash, dto.Password);
            if (!validPassword)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(ToUserResponse(user));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(ToUserResponse(user));
        }

        [HttpPut("user/{userId}/profile")]
        public async Task<IActionResult> UpdateProfile(int userId, UpdateUserProfileDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest("Full name is required.");
            }

            if (!string.IsNullOrWhiteSpace(dto.AlternativeEmail))
            {
                var alternativeEmail = dto.AlternativeEmail.Trim().ToLower();

                if (!IsValidEmail(alternativeEmail))
                {
                    return BadRequest("Please enter a valid alternative email address.");
                }

                if (alternativeEmail == user.Email)
                {
                    return BadRequest("Alternative email must be different from your main email.");
                }
            }

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty;
            user.AlternativeEmail = dto.AlternativeEmail?.Trim().ToLower() ?? string.Empty;
            user.Location = dto.Location?.Trim() ?? string.Empty;
            user.CareerLevel = dto.CareerLevel?.Trim() ?? string.Empty;
            user.PreferredJobType = dto.PreferredJobType?.Trim() ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(ToUserResponse(user));
        }

        [HttpPut("user/{userId}/password")]
        public async Task<IActionResult> ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                return BadRequest("All password fields are required.");
            }

            var currentPasswordIsValid = _passwordService.VerifyPassword(
                user.PasswordHash,
                dto.CurrentPassword);

            if (!currentPasswordIsValid)
            {
                return BadRequest("Current password is incorrect.");
            }

            if (dto.NewPassword.Length < 6)
            {
                return BadRequest("New password must have at least 6 characters.");
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest("New password and confirmation do not match.");
            }

            if (dto.CurrentPassword == dto.NewPassword)
            {
                return BadRequest("New password must be different from the current password.");
            }

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Password changed successfully."
            });
        }

        private static object ToUserResponse(User user)
        {
            return new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.AlternativeEmail,
                user.Location,
                user.CareerLevel,
                user.PreferredJobType
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}