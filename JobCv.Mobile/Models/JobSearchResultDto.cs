using System.Globalization;
using System.Text.RegularExpressions;

namespace JobCv.Mobile.Models
{
    public class JobSearchResultDto
    {
        public string ExternalId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ApplyUrl { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Salary { get; set; } = string.Empty;

        public string DisplaySalary => FormatSalaryForDisplay(Salary, Location);

        public DateTime? PostedAt { get; set; }

        public string PostedText
        {
            get
            {
                if (!PostedAt.HasValue)
                    return "Posted date not available";

                return $"Posted {PostedAt.Value:MMM dd, yyyy}";
            }
        }

        private static string FormatSalaryForDisplay(string? salary, string? location)
        {
            if (string.IsNullOrWhiteSpace(salary))
                return "Not specified";

            var value = salary.Trim();

            if (value.Equals("Not specified", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return "Not specified";
            }

            if (ContainsCurrency(value))
                return value;

            var currency = GetCurrencyPrefix(location);

            var formatted = Regex.Replace(
                value,
                @"(?<![\p{L}\p{Sc}])\d[\d,]*(?:\.\d+)?",
                match => FormatSalaryNumber(match.Value, currency));

            return string.IsNullOrWhiteSpace(formatted)
                ? "Not specified"
                : formatted;
        }

        private static bool ContainsCurrency(string value)
        {
            return value.Contains('£') ||
                   value.Contains('€') ||
                   value.Contains('$') ||
                   value.Contains("GBP", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains("EUR", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains("USD", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains("RON", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains("LEI", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetCurrencyPrefix(string? location)
        {
            var normalizedLocation = location?.Trim() ?? string.Empty;

            if (ContainsAny(normalizedLocation, "romania", "românia", "bucharest", "bucuresti", "bucurești", "cluj", "iasi", "iași", "timisoara", "timișoara", "brasov", "brașov"))
                return "RON ";

            if (ContainsAny(normalizedLocation, "ireland", "dublin", "germany", "france", "spain", "italy", "netherlands", "belgium", "austria", "portugal"))
                return "€";

            if (ContainsAny(normalizedLocation, "united states", "usa", "u.s.", "new york", "california", "texas"))
                return "$";

            return "£";
        }

        private static bool ContainsAny(string value, params string[] keywords)
        {
            return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static string FormatSalaryNumber(string rawNumber, string currencyPrefix)
        {
            var normalizedNumber = rawNumber.Replace(",", string.Empty);

            if (!decimal.TryParse(normalizedNumber, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                return $"{currencyPrefix}{rawNumber}";

            if (number <= 0)
                return rawNumber;

            var numberText = number % 1 == 0
                ? number.ToString("N0", CultureInfo.InvariantCulture)
                : number.ToString("N2", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

            return $"{currencyPrefix}{numberText}";
        }
    }
}
