using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace JobCv.Mobile.Pages
{
    public partial class CareerTestResultPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;
        private readonly CareerTestResult _result;

        private bool _hasDownloadedPdf = false;

        public CareerTestResultPage(
            UserDto user,
            ApiService apiService,
            CareerTestResult result)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _result = result;

            RenderResult();
        }

        private void RenderResult()
        {
            ProfileTitleLabel.Text = _result.ProfileTitle;
            SummaryLabel.Text = _result.Summary;

            ScoresContainer.Children.Clear();
            RolesContainer.Children.Clear();
            StepsContainer.Children.Clear();

            foreach (var score in _result.AreaScores.OrderByDescending(x => x.Score))
            {
                ScoresContainer.Children.Add(CreateScoreView(score));
            }

            foreach (var role in _result.RecommendedRoles)
            {
                RolesContainer.Children.Add(CreateBulletLabel(role));
            }

            foreach (var step in _result.NextSteps)
            {
                StepsContainer.Children.Add(CreateBulletLabel(step));
            }
        }

        private View CreateScoreView(CareerAreaScore score)
        {
            var progress = Math.Clamp(score.Score / 100.0, 0, 1);

            return new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 18
                },
                Padding = 12,
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = GridLength.Star },
                                new ColumnDefinition { Width = GridLength.Auto }
                            },
                            Children =
                            {
                                new Label
                                {
                                    Text = score.Area,
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#0F172A"),
                                    LineBreakMode = LineBreakMode.WordWrap
                                },
                                new Label
                                {
                                    Text = $"{score.Score}%",
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#1D4ED8"),
                                    HorizontalOptions = LayoutOptions.End
                                }
                            }
                        },
                        new ProgressBar
                        {
                            Progress = progress,
                            ProgressColor = Color.FromArgb("#1D4ED8"),
                            BackgroundColor = Color.FromArgb("#E2E8F0")
                        },
                        new Label
                        {
                            Text = score.Description,
                            FontSize = 13,
                            TextColor = Color.FromArgb("#64748B"),
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
                }
            };
        }

        private static Label CreateBulletLabel(string text)
        {
            return new Label
            {
                Text = $"• {text}",
                FontSize = 14,
                TextColor = Color.FromArgb("#334155"),
                LineBreakMode = LineBreakMode.WordWrap
            };
        }

        private async void OnDownloadPdfClicked(object sender, EventArgs e)
        {
            try
            {
                MessageLabel.IsVisible = true;
                MessageLabel.TextColor = Colors.Gray;
                MessageLabel.Text = "Generating PDF report...";

                var request = new CareerTestPdfRequest
                {
                    ProfileTitle = _result.ProfileTitle,
                    Summary = _result.Summary,
                    GeneratedAt = DateTime.UtcNow,
                    AreaScores = _result.AreaScores,
                    RecommendedRoles = _result.RecommendedRoles,
                    NextSteps = _result.NextSteps
                };

                var pdfBytes = await _apiService.ExportCareerTestPdfAsync(request);

                var safeTitle = string.IsNullOrWhiteSpace(_result.ProfileTitle)
                    ? "career_test"
                    : string.Join("_", _result.ProfileTitle.Split(Path.GetInvalidFileNameChars()));

                var fileName = $"career_orientation_test_{safeTitle}.pdf";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);

                _hasDownloadedPdf = true;

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = "PDF report generated. You can save or share it from the PDF viewer.";
            }
            catch (Exception ex)
            {
                MessageLabel.IsVisible = true;
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            if (!_hasDownloadedPdf)
            {
                var confirm = await DisplayAlert(
                    "Leave without PDF?",
                    "This result is not saved. If you leave now without downloading the PDF, you may lose it.",
                    "Leave",
                    "Stay");

                if (!confirm)
                    return;
            }

            await Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!_hasDownloadedPdf)
                {
                    var confirm = await DisplayAlert(
                        "Leave without PDF?",
                        "This result is not saved. If you leave now without downloading the PDF, you may lose it.",
                        "Leave",
                        "Stay");

                    if (!confirm)
                        return;
                }

                await Navigation.PopAsync();
            });

            return true;
        }
    }
}