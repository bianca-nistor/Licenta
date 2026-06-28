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
        private readonly string _baseSourceAttribution;

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

            if (string.IsNullOrWhiteSpace(_result.Language))
                _result.Language = Preferences.Get("AppLanguage", "en") == "ro" ? "ro" : "en";

            _baseSourceAttribution = _result.SourceAttribution ?? string.Empty;

            ApplyStaticLanguageText();
            RenderResult();
        }

        private bool IsRomanian => _result.Language == "ro";

        private void ApplyStaticLanguageText()
        {
            if (IsRomanian)
            {
                TitleLabel.Text = "Rezultatul testului de carieră";
                SubtitleLabel.Text = "Verifică rezultatul și exportă-l ca raport PDF.";
                TemporaryTitleLabel.Text = "Rezultat temporar";
                TemporaryTextLabel.Text = "Acest rezultat nu este salvat automat. Descarcă raportul PDF dacă vrei să îl păstrezi după ce părăsești pagina.";
                ProfileSectionLabel.Text = "Profilul tău";
                ScoresSectionLabel.Text = "Scoruri pe arii de interes";
                RolesSectionLabel.Text = "Recomandări AI";
                StepsSectionLabel.Text = "Pași recomandați";
                SourceTitleLabel.Text = "Notă privind sursa";

                GenerateAiRecommendationsButton.Text = "Generează recomandări AI";
                DownloadButton.Text = "Descarcă raportul PDF";
            }
            else
            {
                TitleLabel.Text = "Career Test Result";
                SubtitleLabel.Text = "Review your result and export it as a PDF report.";
                TemporaryTitleLabel.Text = "Temporary result";
                TemporaryTextLabel.Text = "This result is not saved automatically. Download the PDF report if you want to keep it after leaving this page.";
                ProfileSectionLabel.Text = "Your profile";
                ScoresSectionLabel.Text = "Career area scores";
                RolesSectionLabel.Text = "AI recommendations";
                StepsSectionLabel.Text = "Suggested next steps";
                SourceTitleLabel.Text = "Source note";

                GenerateAiRecommendationsButton.Text = "Generate AI recommendations";
                DownloadButton.Text = "Download PDF report";
            }
        }

        private void RenderResult()
        {
            ProfileTitleLabel.Text = _result.ProfileTitle;
            SummaryLabel.Text = _result.Summary;
            SourceTextLabel.Text = _result.SourceAttribution;

            ScoresContainer.Children.Clear();
            RolesContainer.Children.Clear();
            StepsContainer.Children.Clear();

            foreach (var score in _result.AreaScores
                         .OrderByDescending(x => x.RawScore)
                         .ThenByDescending(x => x.Score))
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

        private async void OnGenerateAiRecommendationsClicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            try
            {
                if (button != null)
                    button.IsEnabled = false;

                MessageLabel.IsVisible = true;
                MessageLabel.TextColor = Colors.Gray;
                MessageLabel.Text = IsRomanian
                    ? "Se generează recomandările AI..."
                    : "Generating AI recommendations...";

                var request = BuildAiRecommendationsRequest();

                var aiResult = await _apiService.GenerateCareerAiRecommendationsAsync(request);

                RenderAiRecommendations(aiResult);
                UpdateResultForPdf(aiResult);

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = IsRomanian
                    ? "Recomandările au fost generate cu succes."
                    : "Recommendations generated successfully.";
            }
            catch (Exception ex)
            {
                MessageLabel.IsVisible = true;
                MessageLabel.TextColor = Colors.Red;
                MessageLabel.Text = ex.Message;
            }
            finally
            {
                if (button != null)
                    button.IsEnabled = true;
            }
        }

        private CareerAiRecommendationsRequest BuildAiRecommendationsRequest()
        {
            return new CareerAiRecommendationsRequest
            {
                ProfileTitle = _result.ProfileTitle,
                ProfileCode = _result.ProfileCode,
                Language = _result.Language,
                Summary = _result.Summary,
                AreaScores = _result.AreaScores.Select(score => new CareerAiAreaScore
                {
                    Area = score.Area,
                    AreaCode = score.AreaCode,
                    Score = score.Score,
                    RawScore = score.RawScore,
                    MaxScore = score.MaxScore,
                    Description = score.Description
                }).ToList()
            };
        }

        private void RenderAiRecommendations(CareerAiRecommendationsResponse aiResult)
        {
            RolesContainer.Children.Clear();
            StepsContainer.Children.Clear();

            if (!string.IsNullOrWhiteSpace(aiResult.Summary))
            {
                RolesContainer.Children.Add(new Label
                {
                    Text = aiResult.Summary,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#334155"),
                    LineBreakMode = LineBreakMode.WordWrap,
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }

            if (aiResult.CareerDirections != null && aiResult.CareerDirections.Count > 0)
            {
                RolesContainer.Children.Add(CreateSectionMiniTitle(IsRomanian
                    ? "Direcții de carieră"
                    : "Career directions"));

                foreach (var direction in aiResult.CareerDirections)
                {
                    if (!string.IsNullOrWhiteSpace(direction))
                        RolesContainer.Children.Add(CreateBulletLabel(direction));
                }
            }

            if (aiResult.RecommendedRoles != null && aiResult.RecommendedRoles.Count > 0)
            {
                RolesContainer.Children.Add(CreateSectionMiniTitle(IsRomanian
                    ? "Roluri posibile"
                    : "Possible roles"));

                foreach (var role in aiResult.RecommendedRoles)
                {
                    if (string.IsNullOrWhiteSpace(role.Title))
                        continue;

                    var text = string.IsNullOrWhiteSpace(role.Reason)
                        ? role.Title
                        : $"{role.Title} - {role.Reason}";

                    RolesContainer.Children.Add(CreateBulletLabel(text));
                }
            }

            if (aiResult.SkillsToDevelop != null && aiResult.SkillsToDevelop.Count > 0)
            {
                RolesContainer.Children.Add(CreateSectionMiniTitle(IsRomanian
                    ? "Competențe de dezvoltat"
                    : "Skills to develop"));

                foreach (var skill in aiResult.SkillsToDevelop)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                        RolesContainer.Children.Add(CreateBulletLabel(skill));
                }
            }

            if (aiResult.NextSteps != null && aiResult.NextSteps.Count > 0)
            {
                foreach (var step in aiResult.NextSteps)
                {
                    if (!string.IsNullOrWhiteSpace(step))
                        StepsContainer.Children.Add(CreateBulletLabel(step));
                }
            }
            else
            {
                foreach (var step in _result.NextSteps)
                {
                    StepsContainer.Children.Add(CreateBulletLabel(step));
                }
            }

            if (!string.IsNullOrWhiteSpace(aiResult.Disclaimer))
            {
                var sourceText = string.IsNullOrWhiteSpace(_baseSourceAttribution)
                    ? aiResult.Disclaimer
                    : $"{_baseSourceAttribution}\n\n{aiResult.Disclaimer}";

                SourceTextLabel.Text = sourceText;
            }
        }

        private void UpdateResultForPdf(CareerAiRecommendationsResponse aiResult)
        {
            if (!string.IsNullOrWhiteSpace(aiResult.Summary))
            {
                _result.Summary = aiResult.Summary;
                SummaryLabel.Text = aiResult.Summary;
            }

            var pdfRecommendations = new List<string>();

            if (aiResult.CareerDirections != null)
            {
                foreach (var direction in aiResult.CareerDirections)
                {
                    if (!string.IsNullOrWhiteSpace(direction))
                        pdfRecommendations.Add(NormalizeListItem(direction));
                }
            }

            if (aiResult.RecommendedRoles != null)
            {
                foreach (var role in aiResult.RecommendedRoles)
                {
                    if (string.IsNullOrWhiteSpace(role.Title))
                        continue;

                    if (string.IsNullOrWhiteSpace(role.Reason))
                    {
                        pdfRecommendations.Add(NormalizeListItem(role.Title));
                    }
                    else
                    {
                        pdfRecommendations.Add(
                            NormalizeListItem($"{role.Title} - {role.Reason}"));
                    }
                }
            }

            if (aiResult.SkillsToDevelop != null)
            {
                foreach (var skill in aiResult.SkillsToDevelop)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                    {
                        pdfRecommendations.Add(IsRomanian
                            ? $"Competență de dezvoltat: {NormalizeListItem(skill)}"
                            : $"Skill to develop: {NormalizeListItem(skill)}");
                    }
                }
            }

            if (pdfRecommendations.Count > 0)
                _result.RecommendedRoles = pdfRecommendations;

            if (aiResult.NextSteps != null && aiResult.NextSteps.Count > 0)
            {
                _result.NextSteps = aiResult.NextSteps
                    .Where(step => !string.IsNullOrWhiteSpace(step))
                    .Select(NormalizeListItem)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(aiResult.Disclaimer))
            {
                _result.SourceAttribution = string.IsNullOrWhiteSpace(_baseSourceAttribution)
                    ? aiResult.Disclaimer
                    : $"{_baseSourceAttribution}\n\n{aiResult.Disclaimer}";
            }
        }
        private View CreateScoreView(CareerAreaScore score)
        {
            var progress = score.MaxScore > 0
                ? Math.Clamp((double)score.RawScore / score.MaxScore, 0, 1)
                : Math.Clamp(score.Score / 100.0, 0, 1);

            var scoreText = score.MaxScore > 0
                ? $"{score.RawScore}/{score.MaxScore} ({score.Score}%)"
                : $"{score.Score}%";

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
                                    Text = string.IsNullOrWhiteSpace(score.AreaCode)
                                        ? score.Area
                                        : $"{score.AreaCode} - {score.Area}",
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#0F172A"),
                                    LineBreakMode = LineBreakMode.WordWrap
                                },
                                new Label
                                {
                                    Text = scoreText,
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

        private static Label CreateSectionMiniTitle(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A"),
                Margin = new Thickness(0, 10, 0, 4)
            };
        }

        private static View CreateBulletLabel(string text)
        {
            var cleanText = NormalizeListItem(text);

            var bullet = new Border
            {
                WidthRequest = 7,
                HeightRequest = 7,
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#1D4ED8"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 4
                },
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 7, 0, 0)
            };

            var label = new Label
            {
                Text = cleanText,
                FontSize = 14,
                TextColor = Color.FromArgb("#334155"),
                LineBreakMode = LineBreakMode.WordWrap
            };

            var grid = new Grid
            {
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = 16 },
            new ColumnDefinition { Width = GridLength.Star }
        },
                ColumnSpacing = 8
            };

            Grid.SetColumn(bullet, 0);
            Grid.SetColumn(label, 1);

            grid.Children.Add(bullet);
            grid.Children.Add(label);

            return grid;
        }
        private static string NormalizeListItem(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var clean = text.Trim();

            while (clean.Length > 0)
            {
                var first = clean[0];

                if (first is '•' or '-' or '*' or '●' or '▪' or '▸' or '►' or '□' or '■' or '☐' or '–' or '—')
                {
                    clean = clean.Substring(1).TrimStart();
                    continue;
                }

                break;
            }

            return clean
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }

        private async void OnDownloadPdfClicked(object sender, EventArgs e)
        {
            try
            {
                MessageLabel.IsVisible = true;
                MessageLabel.TextColor = Colors.Gray;
                MessageLabel.Text = IsRomanian
                    ? "Se generează raportul PDF..."
                    : "Generating PDF report...";

                var request = new CareerTestPdfRequest
                {
                    ProfileTitle = _result.ProfileTitle,
                    ProfileCode = _result.ProfileCode,
                    Language = _result.Language,
                    Summary = _result.Summary,
                    SourceAttribution = _result.SourceAttribution,
                    GeneratedAt = DateTime.UtcNow,
                    AreaScores = _result.AreaScores,
                    RecommendedRoles = _result.RecommendedRoles,
                    NextSteps = _result.NextSteps
                };

                var pdfBytes = await _apiService.ExportCareerTestPdfAsync(request);

                var safeTitle = string.IsNullOrWhiteSpace(_result.ProfileTitle)
                    ? "career_test"
                    : string.Join("_", _result.ProfileTitle.Split(Path.GetInvalidFileNameChars()));

                var fileName = $"career_interest_profile_{safeTitle}.pdf";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);

                _hasDownloadedPdf = true;

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });

                MessageLabel.TextColor = Colors.Green;
                MessageLabel.Text = IsRomanian
                    ? "Raportul PDF a fost generat. Îl poți salva sau distribui din vizualizatorul PDF."
                    : "PDF report generated. You can save or share it from the PDF viewer.";
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
                    IsRomanian ? "Ieși fără PDF?" : "Leave without PDF?",
                    IsRomanian
                        ? "Acest rezultat nu este salvat. Dacă ieși acum fără să descarci PDF-ul, îl poți pierde."
                        : "This result is not saved. If you leave now without downloading the PDF, you may lose it.",
                    IsRomanian ? "Ieși" : "Leave",
                    IsRomanian ? "Rămâi" : "Stay");

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
                        IsRomanian ? "Ieși fără PDF?" : "Leave without PDF?",
                        IsRomanian
                            ? "Acest rezultat nu este salvat. Dacă ieși acum fără să descarci PDF-ul, îl poți pierde."
                            : "This result is not saved. If you leave now without downloading the PDF, you may lose it.",
                        IsRomanian ? "Ieși" : "Leave",
                        IsRomanian ? "Rămâi" : "Stay");

                    if (!confirm)
                        return;
                }

                await Navigation.PopAsync();
            });

            return true;
        }
    }
}