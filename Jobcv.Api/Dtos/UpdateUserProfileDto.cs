namespace JobCv.Api.Dtos
{
    public class UpdateUserProfileDto
    {
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string AlternativeEmail { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string CareerLevel { get; set; } = string.Empty;

        public string PreferredJobType { get; set; } = string.Empty;
    }
}