using System.ComponentModel.DataAnnotations;
namespace JobCv.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string AlternativeEmail { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string CareerLevel { get; set; } = string.Empty;

        public string PreferredJobType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Cv> Cvs { get; set; } = new();
    }
}