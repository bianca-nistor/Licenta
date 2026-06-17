using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using System.Reflection;

namespace JobCv.Mobile.Pages
{
    public partial class CvTemplatePreviewPage : ContentPage
    {
        private readonly int _cvId;
        private readonly ApiService _apiService;

        private CvDto? _cv;

        private Color _primaryColor = Color.FromArgb("#1D4ED8");
        private Color _secondaryColor = Color.FromArgb("#EFF6FF");
        private Color _accentColor = Color.FromArgb("#1D4ED8");
        private Color _sectionTitleColor = Color.FromArgb("#0F172A");

        public CvTemplatePreviewPage(int cvId, ApiService apiService)
        {
            InitializeComponent();

            _cvId = cvId;
            _apiService = apiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCvPreviewAsync();
        }

        private async Task LoadCvPreviewAsync()
        {
            try
            {
                MessageLabel.IsVisible = false;
                MessageLabel.Text = "";

                _cv = await _apiService.GetCvByIdAsync(_cvId);

                if (_cv == null)
                {
                    MessageLabel.Text = "The CV could not be loaded.";
                    MessageLabel.IsVisible = true;
                    return;
                }

                ApplyTemplate(_cv.TemplateName);
                RenderCv(_cv);
            }
            catch (Exception ex)
            {
                MessageLabel.Text = ex.Message;
                MessageLabel.IsVisible = true;
            }
        }

        private void ApplyTemplate(string? templateName)
        {
            var template = NormalizeTemplateName(templateName);

            TemplateLabel.Text = $"Template: {template}";

            switch (template)
            {
                case "green-professional":
                    _primaryColor = Color.FromArgb("#174C35");
                    _secondaryColor = Color.FromArgb("#E9F7EF");
                    _accentColor = Color.FromArgb("#166534");
                    _sectionTitleColor = Color.FromArgb("#174C35");
                    break;

                case "classic-minimal":
                    _primaryColor = Color.FromArgb("#111827");
                    _secondaryColor = Color.FromArgb("#F8FAFC");
                    _accentColor = Color.FromArgb("#64748B");
                    _sectionTitleColor = Color.FromArgb("#111827");
                    break;

                case "blue-sidebar":
                    _primaryColor = Color.FromArgb("#1D4ED8");
                    _secondaryColor = Color.FromArgb("#EFF6FF");
                    _accentColor = Color.FromArgb("#2563EB");
                    _sectionTitleColor = Color.FromArgb("#0F172A");
                    break;

                case "warm-beige":
                    _primaryColor = Color.FromArgb("#8B6246");
                    _secondaryColor = Color.FromArgb("#F3E7D9");
                    _accentColor = Color.FromArgb("#7C4A35");
                    _sectionTitleColor = Color.FromArgb("#6B3F2D");
                    break;

                case "modern-blue":
                default:
                    _primaryColor = Color.FromArgb("#0F172A");
                    _secondaryColor = Color.FromArgb("#EFF6FF");
                    _accentColor = Color.FromArgb("#2563EB");
                    _sectionTitleColor = Color.FromArgb("#0F172A");
                    break;
            }
        }

        private string NormalizeTemplateName(string? templateName)
        {
            var template = string.IsNullOrWhiteSpace(templateName)
                ? "modern-blue"
                : templateName.Trim().ToLowerInvariant();

            return template switch
            {
                "professional-green" => "green-professional",
                "elegant-green" => "green-professional",
                "modern-green" => "green-professional",

                "classic-gray" => "classic-minimal",
                "classic-grey" => "classic-minimal",
                "minimal-classic" => "classic-minimal",

                "sidebar-blue" => "blue-sidebar",

                "beige-warm" => "warm-beige",

                _ => template
            };
        }

        private void RenderCv(CvDto cv)
        {
            PreviewContainer.Children.Clear();

            var template = NormalizeTemplateName(cv.TemplateName);

            PageTitleLabel.Text = string.IsNullOrWhiteSpace(cv.Title)
                ? "CV Preview"
                : cv.Title;

            switch (template)
            {
                case "green-professional":
                    RenderGreenProfessional(cv);
                    break;

                case "classic-minimal":
                    RenderClassicMinimal(cv);
                    break;

                case "blue-sidebar":
                    RenderBlueSidebar(cv);
                    break;

                case "warm-beige":
                    RenderWarmBeige(cv);
                    break;

                case "modern-blue":
                default:
                    RenderModernBlue(cv);
                    break;
            }
        }

        private void RenderModernBlue(CvDto cv)
        {
            CvHeaderBorder.IsVisible = true;
            CvHeaderBorder.BackgroundColor = Color.FromArgb("#0F172A");

            FullNameLabel.Text = GetDisplayName(cv);
            ContactLabel.Text = BuildContactText(cv);
            LinksLabel.Text = BuildLinksText(cv);
            LinksLabel.IsVisible = !string.IsNullOrWhiteSpace(LinksLabel.Text);

            SetPhoto(cv);

            AddSummarySection(cv);
            AddListSection("Skills", cv.Skills);
            AddListSection("Experience", cv.Experiences);
            AddListSection("Education", cv.Educations);
            AddListSection("Projects", cv.Projects);
            AddListSection("Languages", cv.Languages);
            AddListSection("Certifications", cv.Certifications);
        }

        private void RenderGreenProfessional(CvDto cv)
        {
            CvHeaderBorder.IsVisible = true;
            CvHeaderBorder.BackgroundColor = Color.FromArgb("#174C35");

            FullNameLabel.Text = GetDisplayName(cv);
            ContactLabel.Text = BuildContactText(cv);
            LinksLabel.Text = BuildLinksText(cv);
            LinksLabel.IsVisible = !string.IsNullOrWhiteSpace(LinksLabel.Text);

            SetPhoto(cv);

            var twoColumns = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 18
            };

            var left = new VerticalStackLayout { Spacing = 16 };
            var right = new VerticalStackLayout { Spacing = 16 };

            left.Children.Add(CreateSimpleSection("Profile", GetSummaryText(cv)));
            AddGenericSectionToStack(left, "Experience", cv.Experiences);
            AddGenericSectionToStack(left, "Projects", cv.Projects);

            AddGenericSectionToStack(right, "Skills", cv.Skills);
            AddGenericSectionToStack(right, "Education", cv.Educations);
            AddGenericSectionToStack(right, "Languages", cv.Languages);
            AddGenericSectionToStack(right, "Certifications", cv.Certifications);

            twoColumns.Add(left, 0, 0);
            twoColumns.Add(right, 1, 0);

            PreviewContainer.Children.Add(twoColumns);
        }

        private void RenderClassicMinimal(CvDto cv)
        {
            CvHeaderBorder.IsVisible = false;

            PreviewContainer.Children.Add(new Label
            {
                Text = GetDisplayName(cv),
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            PreviewContainer.Children.Add(new Label
            {
                Text = BuildClassicSubtitle(cv),
                FontSize = 15,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            AddClassicSection("Professional Summary", GetSummaryText(cv));
            AddClassicSection("Skills", BuildInlineList(cv.Skills));
            AddClassicSection("Experience", BuildMultilineList(cv.Experiences));
            AddClassicSection("Education", BuildMultilineList(cv.Educations));
            AddClassicSection("Projects", BuildMultilineList(cv.Projects));
            AddClassicSection("Languages", BuildInlineList(cv.Languages));
            AddClassicSection("Certifications", BuildMultilineList(cv.Certifications));
        }

        private void RenderBlueSidebar(CvDto cv)
        {
            CvHeaderBorder.IsVisible = false;

            // The Blue Sidebar template is a two-column CV. On a phone screen the
            // card is too narrow for two readable columns, so the preview uses a
            // fixed document-like width and allows horizontal scrolling. This keeps
            // the same visual style as the exported PDF without breaking words into
            // one-letter lines.
            var documentWidth = DeviceInfo.Idiom == DeviceIdiom.Phone ? 520 : -1;

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(190) },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 24,
                WidthRequest = documentWidth
            };

            var sidebar = new VerticalStackLayout
            {
                Spacing = 18,
                Padding = new Thickness(18, 22),
                BackgroundColor = Color.FromArgb("#1D4ED8")
            };

            sidebar.Children.Add(CreateInitialsCircle(cv));
            sidebar.Children.Add(CreateSidebarSection("CONTACT", BuildContactLines(cv)));
            sidebar.Children.Add(CreateSidebarSection("SKILLS", BuildLines(cv.Skills)));
            sidebar.Children.Add(CreateSidebarSection("LANGUAGES", BuildLines(cv.Languages)));

            var main = new VerticalStackLayout
            {
                Spacing = 16,
                Padding = new Thickness(0, 8, 18, 8)
            };

            main.Children.Add(new Label
            {
                Text = GetDisplayName(cv),
                FontSize = 26,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            var subtitle = BuildBlueSidebarSubtitle(cv);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                main.Children.Add(new Label
                {
                    Text = subtitle,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#2563EB"),
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.WordWrap
                });
            }

            main.Children.Add(CreateHorizontalLine());

            main.Children.Add(CreateSimpleSection("PROFILE", GetSummaryText(cv)));
            AddGenericSectionToStack(main, "EXPERIENCE", cv.Experiences);
            AddGenericSectionToStack(main, "EDUCATION", cv.Educations);
            AddGenericSectionToStack(main, "PROJECTS", cv.Projects);
            AddGenericSectionToStack(main, "CERTIFICATIONS", cv.Certifications);

            grid.Add(sidebar, 0, 0);
            grid.Add(main, 1, 0);

            PreviewContainer.Children.Add(new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                Content = grid
            });
        }

        private void RenderWarmBeige(CvDto cv)
        {
            CvHeaderBorder.IsVisible = true;
            CvHeaderBorder.BackgroundColor = Color.FromArgb("#8B6246");

            FullNameLabel.Text = GetDisplayName(cv);
            ContactLabel.Text = BuildContactText(cv);
            LinksLabel.Text = BuildLinksText(cv);
            LinksLabel.IsVisible = !string.IsNullOrWhiteSpace(LinksLabel.Text);

            SetPhoto(cv);

            PreviewContainer.BackgroundColor = Color.FromArgb("#F3E7D9");

            var twoColumns = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 18
            };

            var left = new VerticalStackLayout { Spacing = 16 };
            var right = new VerticalStackLayout { Spacing = 16 };

            left.Children.Add(CreateSimpleSection("Profile", GetSummaryText(cv)));
            AddGenericSectionToStack(left, "Experience", cv.Experiences);
            AddGenericSectionToStack(left, "Projects", cv.Projects);

            AddGenericSectionToStack(right, "Skills", cv.Skills);
            AddGenericSectionToStack(right, "Education", cv.Educations);
            AddGenericSectionToStack(right, "Languages", cv.Languages);
            AddGenericSectionToStack(right, "Certifications", cv.Certifications);

            twoColumns.Add(left, 0, 0);
            twoColumns.Add(right, 1, 0);

            PreviewContainer.Children.Add(twoColumns);
        }

        private void AddSummarySection(CvDto cv)
        {
            AddSection(
                "Professional Summary",
                new Label
                {
                    Text = GetSummaryText(cv),
                    FontSize = 14,
                    TextColor = Color.FromArgb("#334155"),
                    LineBreakMode = LineBreakMode.WordWrap
                });
        }

        private void AddListSection<T>(string title, List<T> items)
        {
            if (items == null || items.Count == 0)
                return;

            var stack = new VerticalStackLayout
            {
                Spacing = 10
            };

            foreach (var item in items)
                stack.Children.Add(CreateItemCard(item!));

            AddSection(title, stack);
        }

        private void AddGenericSectionToStack<T>(VerticalStackLayout stack, string title, List<T> items)
        {
            if (items == null || items.Count == 0)
                return;

            stack.Children.Add(CreateSimpleSection(title, BuildMultilineList(items)));
        }

        private View CreateSimpleSection(string title, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "-";

            var stack = new VerticalStackLayout
            {
                Spacing = 6
            };

            stack.Children.Add(new Label
            {
                Text = title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = _sectionTitleColor,
                LineBreakMode = LineBreakMode.WordWrap
            });

            stack.Children.Add(new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = _accentColor,
                Opacity = 0.35
            });

            stack.Children.Add(new Label
            {
                Text = text,
                FontSize = 14,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            return stack;
        }

        private void AddClassicSection(string title, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            PreviewContainer.Children.Add(new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#CBD5E1")
            });

            PreviewContainer.Children.Add(new Label
            {
                Text = title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            PreviewContainer.Children.Add(new Label
            {
                Text = text,
                FontSize = 14,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        private View CreateItemCard(object item)
        {
            var title = GetFirstValue(
                item,
                "Title",
                "Name",
                "SkillName",
                "Institution",
                "School",
                "Company",
                "LanguageName",
                "CertificationName");

            var subtitle = GetFirstValue(
                item,
                "Subtitle",
                "Degree",
                "FieldOfStudy",
                "Role",
                "Position",
                "Level",
                "Issuer",
                "Technologies",
                "Location");

            var description = GetFirstValue(
                item,
                "Description",
                "Summary",
                "Responsibilities",
                "Details");

            var dates = BuildDateText(item);

            return new Border
            {
                BackgroundColor = _secondaryColor,
                Stroke = _secondaryColor,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 16
                },
                Padding = 14,
                Content = new VerticalStackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label
                        {
                            Text = string.IsNullOrWhiteSpace(title) ? "Untitled item" : title,
                            FontSize = 15,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A"),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = subtitle,
                            FontSize = 13,
                            TextColor = _accentColor,
                            FontAttributes = FontAttributes.Bold,
                            IsVisible = !string.IsNullOrWhiteSpace(subtitle),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = dates,
                            FontSize = 12,
                            TextColor = Color.FromArgb("#64748B"),
                            IsVisible = !string.IsNullOrWhiteSpace(dates),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = description,
                            FontSize = 13,
                            TextColor = Color.FromArgb("#334155"),
                            IsVisible = !string.IsNullOrWhiteSpace(description),
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
                }
            };
        }

        private void AddSection(string title, View content)
        {
            var section = new VerticalStackLayout
            {
                Spacing = 8
            };

            section.Children.Add(new Label
            {
                Text = title,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = _sectionTitleColor
            });

            section.Children.Add(new BoxView
            {
                HeightRequest = 2,
                WidthRequest = 52,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = _accentColor
            });

            section.Children.Add(content);

            PreviewContainer.Children.Add(section);
        }

        private View CreateSidebarSection(string title, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "-";

            return new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text = text,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#DBEAFE"),
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            };
        }

        private View CreateInitialsCircle(CvDto cv)
        {
            return new Border
            {
                WidthRequest = 86,
                HeightRequest = 86,
                BackgroundColor = Color.FromArgb("#DBEAFE"),
                Stroke = Color.FromArgb("#BFDBFE"),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 43
                },
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = GetInitials(cv),
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1D4ED8"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };
        }

        private View CreateHorizontalLine()
        {
            return new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Color.FromArgb("#CBD5E1")
            };
        }

        private void SetPhoto(CvDto cv)
        {
            if (cv.HasPhoto)
            {
                PhotoBorder.IsVisible = true;
                PhotoImage.Source = ImageSource.FromUri(new Uri(_apiService.GetCvPhotoUrl(cv.Id)));
            }
            else
            {
                PhotoBorder.IsVisible = false;
            }
        }

        private string GetDisplayName(CvDto cv)
        {
            return string.IsNullOrWhiteSpace(cv.FullName)
                ? "No name added"
                : cv.FullName;
        }

        private string GetSummaryText(CvDto cv)
        {
            return string.IsNullOrWhiteSpace(cv.Summary)
                ? "No professional summary added yet."
                : cv.Summary.Trim();
        }

        private string BuildClassicSubtitle(CvDto cv)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(cv.Email))
                parts.Add(cv.Email);

            if (!string.IsNullOrWhiteSpace(cv.Phone))
                parts.Add(cv.Phone);

            if (!string.IsNullOrWhiteSpace(cv.Location))
                parts.Add(cv.Location);

            return parts.Count == 0
                ? "No contact information added"
                : string.Join(" | ", parts);
        }


        private string BuildBlueSidebarSubtitle(CvDto cv)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(cv.Location))
                parts.Add(cv.Location);

            if (!string.IsNullOrWhiteSpace(cv.Email))
                parts.Add(cv.Email);

            if (!string.IsNullOrWhiteSpace(cv.Phone))
                parts.Add(cv.Phone);

            return string.Join(" • ", parts);
        }

        private string BuildContactText(CvDto cv)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(cv.Email))
                parts.Add(cv.Email);

            if (!string.IsNullOrWhiteSpace(cv.Phone))
                parts.Add(cv.Phone);

            if (!string.IsNullOrWhiteSpace(cv.Location))
                parts.Add(cv.Location);

            return parts.Count == 0
                ? "No contact information added"
                : string.Join("  ", parts);
        }

        private string BuildContactLines(CvDto cv)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(cv.Email))
                parts.Add(cv.Email);

            if (!string.IsNullOrWhiteSpace(cv.Phone))
                parts.Add(cv.Phone);

            if (!string.IsNullOrWhiteSpace(cv.Location))
                parts.Add(cv.Location);

            return parts.Count == 0
                ? "No contact information added"
                : string.Join("\n", parts);
        }

        private string BuildLinksText(CvDto cv)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(cv.LinkedInUrl))
                parts.Add($"LinkedIn: {cv.LinkedInUrl}");

            if (!string.IsNullOrWhiteSpace(cv.GitHubUrl))
                parts.Add($"GitHub: {cv.GitHubUrl}");

            if (!string.IsNullOrWhiteSpace(cv.PortfolioUrl))
                parts.Add($"Portfolio: {cv.PortfolioUrl}");

            return string.Join("\n", parts);
        }

        private string BuildInlineList<T>(List<T> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            var values = items
                .Select(item => GetPrimaryItemText(item!))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return string.Join("  ", values);
        }

        private string BuildLines<T>(List<T> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            var values = items
                .Select(item => GetPrimaryItemText(item!))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return string.Join("\n", values);
        }

        private string BuildMultilineList<T>(List<T> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            var lines = new List<string>();

            foreach (var item in items)
            {
                var title = GetPrimaryItemText(item!);
                var subtitle = GetSecondaryItemText(item!);
                var dates = BuildDateText(item!);
                var description = GetDescriptionText(item!);

                var itemLines = new List<string>();

                if (!string.IsNullOrWhiteSpace(title))
                    itemLines.Add(title);

                if (!string.IsNullOrWhiteSpace(subtitle))
                    itemLines.Add(subtitle);

                if (!string.IsNullOrWhiteSpace(dates))
                    itemLines.Add(dates);

                if (!string.IsNullOrWhiteSpace(description))
                    itemLines.Add(description);

                if (itemLines.Count > 0)
                    lines.Add(string.Join("\n", itemLines));
            }

            return string.Join("\n\n", lines);
        }

        private string GetPrimaryItemText(object item)
        {
            return GetFirstValue(
                item,
                "Title",
                "Name",
                "SkillName",
                "Institution",
                "School",
                "Company",
                "LanguageName",
                "CertificationName");
        }

        private string GetSecondaryItemText(object item)
        {
            return GetFirstValue(
                item,
                "Subtitle",
                "Degree",
                "FieldOfStudy",
                "Role",
                "Position",
                "Level",
                "Issuer",
                "Technologies",
                "Location");
        }

        private string GetDescriptionText(object item)
        {
            return GetFirstValue(
                item,
                "Description",
                "Summary",
                "Responsibilities",
                "Details");
        }

        private string GetInitials(CvDto cv)
        {
            if (string.IsNullOrWhiteSpace(cv.FullName))
                return "CV";

            var parts = cv.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .ToList();

            if (parts.Count == 0)
                return "CV";

            return string.Join("", parts.Select(x => x[0])).ToUpperInvariant();
        }

        private string GetFirstValue(object item, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var value = GetPropertyValue(item, propertyName);

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }

        private string GetPropertyValue(object item, string propertyName)
        {
            var property = item.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null)
                return string.Empty;

            var value = property.GetValue(item);

            if (value == null)
                return string.Empty;

            if (value is DateTime date)
                return date == DateTime.MinValue
                    ? string.Empty
                    : date.ToString("MMM yyyy");

            return value.ToString() ?? string.Empty;
        }

        private string BuildDateText(object item)
        {
            var start = GetFirstValue(item, "StartDate", "StartedAt", "From", "Start");
            var end = GetFirstValue(item, "EndDate", "EndedAt", "To", "End");

            if (string.IsNullOrWhiteSpace(start) && string.IsNullOrWhiteSpace(end))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(end))
                end = "Present";

            if (string.IsNullOrWhiteSpace(start))
                return end;

            return $"{start} - {end}";
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnExportPdfClicked(object sender, EventArgs e)
        {
            try
            {
                var downloadUrl = _apiService.GetCvPdfDownloadUrl(_cvId);
                await Launcher.OpenAsync(downloadUrl);
            }
            catch
            {
                await DisplayAlert(
                    "Error",
                    "The PDF could not be opened.",
                    "OK");
            }
        }
    }
}