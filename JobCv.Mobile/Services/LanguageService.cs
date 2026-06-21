using System.Globalization;

namespace JobCv.Mobile.Services
{
    public static class LanguageService
    {
        private const string LanguagePreferenceKey = "AppLanguage";

        public static event EventHandler? LanguageChanged;

        public static string CurrentLanguage => Preferences.Get(LanguagePreferenceKey, "en");

        public static bool IsRomanian => CurrentLanguage.Equals("ro", StringComparison.OrdinalIgnoreCase);

        public static void ApplySavedLanguage()
        {
            ApplyCulture(CurrentLanguage);
        }

        public static void SetLanguage(string languageCode)
        {
            languageCode = NormalizeLanguage(languageCode);

            var previousLanguage = CurrentLanguage;

            Preferences.Set(LanguagePreferenceKey, languageCode);
            ApplyCulture(languageCode);

            if (!previousLanguage.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string GetDisplayName(string languageCode)
        {
            return NormalizeLanguage(languageCode) == "ro" ? "Română" : "English";
        }

        private static string NormalizeLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return "en";

            languageCode = languageCode.Trim().ToLowerInvariant();

            return languageCode == "ro" ? "ro" : "en";
        }

        private static void ApplyCulture(string languageCode)
        {
            languageCode = NormalizeLanguage(languageCode);

            var culture = new CultureInfo(languageCode);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
