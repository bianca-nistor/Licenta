using JobCv.Mobile.Services;

namespace JobCv.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            LanguageService.ApplySavedLanguage();

            InitializeComponent();

            MainPage = new LocalizedNavigationPage(new MainPage());
        }
    }
}
