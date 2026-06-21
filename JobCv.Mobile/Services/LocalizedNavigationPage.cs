using Microsoft.Maui.ApplicationModel;

namespace JobCv.Mobile.Services
{
    public class LocalizedNavigationPage : NavigationPage
    {
        public LocalizedNavigationPage(Page root) : base(root)
        {
            LanguageService.LanguageChanged += OnLanguageChanged;
            Pushed += OnPushed;
            Popped += OnPopped;
            PoppedToRoot += OnPoppedToRoot;

            RegisterPage(root);
            ApplyCurrentPageSoon();
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            ApplyCurrentPageSoon();
        }

        private void OnPushed(object? sender, NavigationEventArgs e)
        {
            RegisterPage(e.Page);
            ApplyPageSoon(e.Page);
        }

        private void OnPopped(object? sender, NavigationEventArgs e)
        {
            ApplyCurrentPageSoon();
        }

        private void OnPoppedToRoot(object? sender, NavigationEventArgs e)
        {
            ApplyCurrentPageSoon();
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            ApplyCurrentPageSoon();
        }

        private void RegisterPage(Page page)
        {
            page.Appearing -= OnPageAppearing;
            page.Appearing += OnPageAppearing;

            page.Loaded -= OnPageLoaded;
            page.Loaded += OnPageLoaded;
        }

        private void OnPageAppearing(object? sender, EventArgs e)
        {
            if (sender is Page page)
                ApplyPageSoon(page);
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            if (sender is Page page)
                ApplyPageSoon(page);
        }

        private void ApplyCurrentPageSoon()
        {
            if (CurrentPage != null)
                ApplyPageSoon(CurrentPage);
        }

        private static void ApplyPageSoon(Page page)
        {
            MainThread.BeginInvokeOnMainThread(() => UiTranslationService.ApplyToPage(page));

            _ = Task.Run(async () =>
            {
                foreach (var delay in new[] { 150, 500, 1200 })
                {
                    await Task.Delay(delay);
                    MainThread.BeginInvokeOnMainThread(() => UiTranslationService.ApplyToPage(page));
                }
            });
        }
    }
}
