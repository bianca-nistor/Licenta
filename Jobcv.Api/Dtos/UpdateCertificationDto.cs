namespace JobCv.Api.Dtos
{
    public class UpdateCertificationDto
    {
        public string Name { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public DateTime? Date { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}