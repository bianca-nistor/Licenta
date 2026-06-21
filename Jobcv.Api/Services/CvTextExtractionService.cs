using System.Text;
using UglyToad.PdfPig;

namespace JobCv.Api.Services
{
    public interface ICvTextExtractionService
    {
        Task<string> ExtractTextAsync(string filePath, string originalFileName);
    }

    public class CvTextExtractionService : ICvTextExtractionService
    {
        public Task<string> ExtractTextAsync(string filePath, string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Uploaded CV file was not found.", filePath);

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            if (extension != ".pdf")
                throw new NotSupportedException("Only PDF files can be imported into editable CVs.");

            try
            {
                return Task.FromResult(ExtractPdfText(filePath));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("This PDF could not be read. It may be protected, encrypted, damaged, or image-only.", ex);
            }
        }

        private static string ExtractPdfText(string filePath)
        {
            var builder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    builder.AppendLine(page.Text);
                    builder.AppendLine();
                }
            }

            return builder.ToString().Trim();
        }
    }
}
