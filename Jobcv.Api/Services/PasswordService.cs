using Microsoft.AspNetCore.Identity;

namespace JobCv.Api.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<string> _hasher = new();

        public string HashPassword(string password)
        {
            return _hasher.HashPassword("user", password);
        }

        public bool VerifyPassword(string hash, string password)
        {
            var result = _hasher.VerifyHashedPassword("user", hash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}