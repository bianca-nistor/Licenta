using JobCv.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JobCv.Api.Pdf
{
    public class CvPdfDocument : IDocument
    {
        private readonly Cv _cv;
        private readonly TemplateStyle _style;

        public CvPdfDocument(Cv cv)
        {
            _cv = cv;
            _style = GetTemplateStyle(cv.TemplateName);
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            if (_style.Id == "blue-sidebar")
            {
                ComposeBlueSidebar(container);
                return;
            }

            ComposeStandardLayout(container);
        }

        private void ComposeStandardLayout(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(_style.TextColor));

                page.Content()
                    .Background(_style.PageBackground)
                    .Column(column =>
                    {
                        ComposeHeader(column);

                        ComposeSummary(column);
                        ComposeSkills(column);
                        ComposeExperience(column);
                        ComposeEducation(column);
                        ComposeProjects(column);
                        ComposeLanguages(column);
                        ComposeCertifications(column);
                    });
            });
        }

        private void ComposeBlueSidebar(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor("#334155"));

                page.Content().Row(row =>
                {
                    row.ConstantItem(180)
                        .Background("#1D4ED8")
                        .Padding(16)
                        .Column(sidebar =>
                        {
                            sidebar.Spacing(12);

                            ComposeSidebarPhotoOrInitials(sidebar);

                            sidebar.Item().Text("CONTACT")
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.White);

                            var contact = GetContactText();

                            if (!string.IsNullOrWhiteSpace(contact))
                            {
                                sidebar.Item().Text(contact.Replace(" | ", "\n"))
                                    .FontSize(8.5f)
                                    .FontColor("#DBEAFE")
                                    .LineHeight(1.25f);
                            }

                            var links = GetLinksText();

                            if (!string.IsNullOrWhiteSpace(links))
                            {
                                sidebar.Item().PaddingTop(6).Text("LINKS")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);

                                sidebar.Item().Text(links.Replace(" | ", "\n"))
                                    .FontSize(7.5f)
                                    .FontColor("#DBEAFE")
                                    .LineHeight(1.25f);
                            }

                            if (_cv.Skills != null && _cv.Skills.Any())
                            {
                                sidebar.Item().PaddingTop(8).Text("SKILLS")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);

                                sidebar.Item().Text(string.Join("\n", _cv.Skills.Select(x => x.Name)))
                                    .FontSize(8.5f)
                                    .FontColor("#DBEAFE")
                                    .LineHeight(1.25f);
                            }

                            if (_cv.Languages != null && _cv.Languages.Any())
                            {
                                sidebar.Item().PaddingTop(8).Text("LANGUAGES")
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);

                                sidebar.Item().Text(string.Join("\n", _cv.Languages.Select(x => $"{x.Name} - {x.Level}")))
                                    .FontSize(8.5f)
                                    .FontColor("#DBEAFE")
                                    .LineHeight(1.25f);
                            }
                        });

                    row.RelativeItem()
                        .PaddingLeft(22)
                        .Column(content =>
                        {
                            content.Spacing(12);

                            content.Item().Text(GetDisplayName())
                                .FontSize(26)
                                .Bold()
                                .FontColor("#0F172A");

                            var titleOrLocation = GetBlueSidebarSubtitle();

                            if (!string.IsNullOrWhiteSpace(titleOrLocation))
                            {
                                content.Item().Text(titleOrLocation)
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor("#1D4ED8");
                            }

                            content.Item().PaddingBottom(4).LineHorizontal(1).LineColor("#CBD5E1");

                            ComposeBlueSidebarSummary(content);
                            ComposeBlueSidebarExperience(content);
                            ComposeBlueSidebarEducation(content);
                            ComposeBlueSidebarProjects(content);
                            ComposeBlueSidebarCertifications(content);
                        });
                });
            });
        }

        private void ComposeSidebarPhotoOrInitials(ColumnDescriptor sidebar)
        {
            var hasPhoto =
                !string.IsNullOrWhiteSpace(_cv.PhotoPath) &&
                File.Exists(_cv.PhotoPath);

            if (hasPhoto)
            {
                sidebar.Item()
                    .AlignCenter()
                    .Width(76)
                    .Height(76)
                    .CornerRadius(38)
                    .Image(_cv.PhotoPath)
                    .FitUnproportionally();

                return;
            }

            sidebar.Item()
                .AlignCenter()
                .Width(76)
                .Height(76)
                .Background("#DBEAFE")
                .CornerRadius(38)
                .AlignCenter()
                .AlignMiddle()
                .Text(GetInitials(GetDisplayName()))
                .FontSize(22)
                .Bold()
                .FontColor("#1D4ED8");
        }

        private void ComposeBlueSidebarSummary(ColumnDescriptor column)
        {
            if (string.IsNullOrWhiteSpace(_cv.Summary))
                return;

            ComposeBlueSidebarSectionTitle(column, "PROFILE");

            column.Item().Text(_cv.Summary)
                .FontSize(10)
                .LineHeight(1.3f)
                .FontColor("#475569");
        }

        private void ComposeBlueSidebarExperience(ColumnDescriptor column)
        {
            if (_cv.Experiences == null || !_cv.Experiences.Any())
                return;

            ComposeBlueSidebarSectionTitle(column, "EXPERIENCE");

            foreach (var item in _cv.Experiences.OrderByDescending(x => x.StartDate))
            {
                column.Item().PaddingBottom(8).Column(exp =>
                {
                    exp.Item().Text($"{item.JobTitle} - {item.Company}")
                        .FontSize(11)
                        .Bold()
                        .FontColor("#0F172A");

                    var dateText = FormatDateRange(item.StartDate, item.EndDate, item.IsCurrent);

                    if (!string.IsNullOrWhiteSpace(dateText))
                    {
                        exp.Item().PaddingTop(2).Text(dateText)
                            .FontSize(9)
                            .FontColor("#64748B");
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        exp.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor("#475569");
                    }
                });
            }
        }

        private void ComposeBlueSidebarEducation(ColumnDescriptor column)
        {
            if (_cv.Educations == null || !_cv.Educations.Any())
                return;

            ComposeBlueSidebarSectionTitle(column, "EDUCATION");

            foreach (var item in _cv.Educations.OrderByDescending(x => x.StartDate))
            {
                column.Item().PaddingBottom(8).Column(education =>
                {
                    education.Item().Text(item.Degree)
                        .FontSize(11)
                        .Bold()
                        .FontColor("#0F172A");

                    if (!string.IsNullOrWhiteSpace(item.Institution))
                    {
                        education.Item().PaddingTop(2).Text(item.Institution)
                            .FontSize(10)
                            .FontColor("#64748B");
                    }

                    var dateText = FormatDateRange(item.StartDate, item.EndDate, false);

                    if (!string.IsNullOrWhiteSpace(dateText))
                    {
                        education.Item().PaddingTop(2).Text(dateText)
                            .FontSize(9)
                            .FontColor("#64748B");
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        education.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor("#475569");
                    }
                });
            }
        }

        private void ComposeBlueSidebarProjects(ColumnDescriptor column)
        {
            if (_cv.Projects == null || !_cv.Projects.Any())
                return;

            ComposeBlueSidebarSectionTitle(column, "PROJECTS");

            foreach (var item in _cv.Projects)
            {
                column.Item().PaddingBottom(8).Column(project =>
                {
                    project.Item().Text(item.Title)
                        .FontSize(11)
                        .Bold()
                        .FontColor("#0F172A");

                    if (!string.IsNullOrWhiteSpace(item.Technologies))
                    {
                        project.Item().PaddingTop(2).Text(item.Technologies)
                            .FontSize(9)
                            .FontColor("#1D4ED8");
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        project.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor("#475569");
                    }

                    var links = new List<string>();

                    if (!string.IsNullOrWhiteSpace(item.ProjectUrl))
                        links.Add($"Project: {item.ProjectUrl}");

                    if (!string.IsNullOrWhiteSpace(item.GitHubUrl))
                        links.Add($"GitHub: {item.GitHubUrl}");

                    if (links.Count > 0)
                    {
                        project.Item().PaddingTop(3).Text(string.Join(" | ", links))
                            .FontSize(9)
                            .FontColor("#64748B");
                    }
                });
            }
        }

        private void ComposeBlueSidebarCertifications(ColumnDescriptor column)
        {
            if (_cv.Certifications == null || !_cv.Certifications.Any())
                return;

            ComposeBlueSidebarSectionTitle(column, "CERTIFICATIONS");

            foreach (var item in _cv.Certifications.OrderByDescending(x => x.Date))
            {
                column.Item().PaddingBottom(8).Column(certification =>
                {
                    certification.Item().Text(item.Name)
                        .FontSize(11)
                        .Bold()
                        .FontColor("#0F172A");

                    var infoParts = new List<string>();

                    if (!string.IsNullOrWhiteSpace(item.Issuer))
                        infoParts.Add(item.Issuer);

                    if (item.Date.HasValue)
                        infoParts.Add(item.Date.Value.ToString("MMM yyyy"));

                    if (infoParts.Count > 0)
                    {
                        certification.Item().PaddingTop(2).Text(string.Join(" | ", infoParts))
                            .FontSize(9)
                            .FontColor("#64748B");
                    }

                    if (!string.IsNullOrWhiteSpace(item.Url))
                    {
                        certification.Item().PaddingTop(3).Text(item.Url)
                            .FontSize(9)
                            .FontColor("#1D4ED8");
                    }
                });
            }
        }

        private void ComposeBlueSidebarSectionTitle(ColumnDescriptor column, string title)
        {
            column.Item().PaddingTop(8).Text(title)
                .FontSize(12)
                .Bold()
                .FontColor("#0F172A");

            column.Item().PaddingTop(2).PaddingBottom(6)
                .LineHorizontal(1)
                .LineColor("#CBD5E1");
        }

        private void ComposeHeader(ColumnDescriptor column)
        {
            var hasPhoto =
                !string.IsNullOrWhiteSpace(_cv.PhotoPath) &&
                File.Exists(_cv.PhotoPath);

            if (_style.Id == "classic")
            {
                column.Item().PaddingBottom(14).Column(classicHeader =>
                {
                    classicHeader.Item().Row(row =>
                    {
                        if (hasPhoto)
                        {
                            row.ConstantItem(86).Column(photoColumn =>
                            {
                                photoColumn.Item()
                                    .Width(74)
                                    .Height(74)
                                    .CornerRadius(37)
                                    .Image(_cv.PhotoPath)
                                    .FitUnproportionally();
                            });
                        }

                        row.RelativeItem()
                            .PaddingLeft(hasPhoto ? 14 : 0)
                            .Column(header =>
                            {
                                header.Item().Text(GetDisplayName())
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(_style.HeaderTextColor);

                                var contact = GetContactText();

                                if (!string.IsNullOrWhiteSpace(contact))
                                {
                                    header.Item().PaddingTop(5).Text(contact)
                                        .FontSize(9)
                                        .FontColor(_style.MutedTextColor);
                                }

                                var links = GetLinksText();

                                if (!string.IsNullOrWhiteSpace(links))
                                {
                                    header.Item().PaddingTop(4).Text(links)
                                        .FontSize(9)
                                        .FontColor(_style.AccentColor);
                                }
                            });
                    });

                    classicHeader.Item()
                        .PaddingTop(14)
                        .LineHorizontal(1)
                        .LineColor(_style.LineColor);
                });

                return;
            }

            column.Item()
                .Background(_style.HeaderBackground)
                .Padding(18)
                .Row(row =>
                {
                    if (hasPhoto)
                    {
                        row.ConstantItem(86).Column(photoColumn =>
                        {
                            photoColumn.Item()
                                .Width(74)
                                .Height(74)
                                .CornerRadius(37)
                                .Image(_cv.PhotoPath)
                                .FitUnproportionally();
                        });
                    }

                    row.RelativeItem()
                        .PaddingLeft(hasPhoto ? 12 : 0)
                        .Column(header =>
                        {
                            header.Item().Text(GetDisplayName())
                                .FontSize(24)
                                .Bold()
                                .FontColor(_style.HeaderTextColor);

                            var contact = GetContactText();

                            if (!string.IsNullOrWhiteSpace(contact))
                            {
                                header.Item().PaddingTop(6).Text(contact)
                                    .FontSize(9)
                                    .FontColor(_style.HeaderMutedTextColor);
                            }

                            var links = GetLinksText();

                            if (!string.IsNullOrWhiteSpace(links))
                            {
                                header.Item().PaddingTop(6).Text(links)
                                    .FontSize(9)
                                    .FontColor(_style.HeaderLinkColor);
                            }
                        });
                });

            column.Item().PaddingBottom(14);
        }

        private void ComposeSummary(ColumnDescriptor column)
        {
            if (string.IsNullOrWhiteSpace(_cv.Summary))
                return;

            ComposeSectionTitle(column, "Professional Summary");

            column.Item().Text(_cv.Summary)
                .FontSize(10)
                .LineHeight(1.3f)
                .FontColor(_style.TextColor);

            column.Item().PaddingBottom(10);
        }

        private void ComposeSkills(ColumnDescriptor column)
        {
            if (_cv.Skills == null || !_cv.Skills.Any())
                return;

            ComposeSectionTitle(column, "Skills");

            column.Item().Text(string.Join("  •  ", _cv.Skills.Select(x => x.Name)))
                .FontSize(10)
                .FontColor(_style.TextColor);

            column.Item().PaddingBottom(10);
        }

        private void ComposeExperience(ColumnDescriptor column)
        {
            if (_cv.Experiences == null || !_cv.Experiences.Any())
                return;

            ComposeSectionTitle(column, "Experience");

            foreach (var item in _cv.Experiences.OrderByDescending(x => x.StartDate))
            {
                column.Item().PaddingBottom(8).Column(exp =>
                {
                    exp.Item().Text($"{item.JobTitle} - {item.Company}")
                        .FontSize(11)
                        .Bold()
                        .FontColor(_style.TextColor);

                    var dateText = FormatDateRange(item.StartDate, item.EndDate, item.IsCurrent);

                    if (!string.IsNullOrWhiteSpace(dateText))
                    {
                        exp.Item().PaddingTop(2).Text(dateText)
                            .FontSize(9)
                            .FontColor(_style.MutedTextColor);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        exp.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor(_style.TextColor);
                    }
                });
            }

            column.Item().PaddingBottom(6);
        }

        private void ComposeEducation(ColumnDescriptor column)
        {
            if (_cv.Educations == null || !_cv.Educations.Any())
                return;

            ComposeSectionTitle(column, "Education");

            foreach (var item in _cv.Educations.OrderByDescending(x => x.StartDate))
            {
                column.Item().PaddingBottom(8).Column(education =>
                {
                    education.Item().Text(item.Degree)
                        .FontSize(11)
                        .Bold()
                        .FontColor(_style.TextColor);

                    if (!string.IsNullOrWhiteSpace(item.Institution))
                    {
                        education.Item().PaddingTop(2).Text(item.Institution)
                            .FontSize(10)
                            .FontColor(_style.MutedTextColor);
                    }

                    var dateText = FormatDateRange(item.StartDate, item.EndDate, false);

                    if (!string.IsNullOrWhiteSpace(dateText))
                    {
                        education.Item().PaddingTop(2).Text(dateText)
                            .FontSize(9)
                            .FontColor(_style.MutedTextColor);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        education.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor(_style.TextColor);
                    }
                });
            }

            column.Item().PaddingBottom(6);
        }

        private void ComposeProjects(ColumnDescriptor column)
        {
            if (_cv.Projects == null || !_cv.Projects.Any())
                return;

            ComposeSectionTitle(column, "Projects");

            foreach (var item in _cv.Projects)
            {
                column.Item().PaddingBottom(8).Column(project =>
                {
                    project.Item().Text(item.Title)
                        .FontSize(11)
                        .Bold()
                        .FontColor(_style.TextColor);

                    if (!string.IsNullOrWhiteSpace(item.Technologies))
                    {
                        project.Item().PaddingTop(2).Text(item.Technologies)
                            .FontSize(9)
                            .FontColor(_style.AccentColor);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Description))
                    {
                        project.Item().PaddingTop(4).Text(item.Description)
                            .FontSize(10)
                            .LineHeight(1.25f)
                            .FontColor(_style.TextColor);
                    }

                    var links = new List<string>();

                    if (!string.IsNullOrWhiteSpace(item.ProjectUrl))
                        links.Add($"Project: {item.ProjectUrl}");

                    if (!string.IsNullOrWhiteSpace(item.GitHubUrl))
                        links.Add($"GitHub: {item.GitHubUrl}");

                    if (links.Count > 0)
                    {
                        project.Item().PaddingTop(3).Text(string.Join(" | ", links))
                            .FontSize(9)
                            .FontColor(_style.MutedTextColor);
                    }
                });
            }

            column.Item().PaddingBottom(6);
        }

        private void ComposeLanguages(ColumnDescriptor column)
        {
            if (_cv.Languages == null || !_cv.Languages.Any())
                return;

            ComposeSectionTitle(column, "Languages");

            column.Item().Column(languages =>
            {
                foreach (var item in _cv.Languages)
                {
                    languages.Item().Text($"{item.Name} - {item.Level}")
                        .FontSize(10)
                        .FontColor(_style.TextColor);
                }
            });

            column.Item().PaddingBottom(10);
        }

        private void ComposeCertifications(ColumnDescriptor column)
        {
            if (_cv.Certifications == null || !_cv.Certifications.Any())
                return;

            ComposeSectionTitle(column, "Certifications");

            foreach (var item in _cv.Certifications.OrderByDescending(x => x.Date))
            {
                column.Item().PaddingBottom(8).Column(certification =>
                {
                    certification.Item().Text(item.Name)
                        .FontSize(11)
                        .Bold()
                        .FontColor(_style.TextColor);

                    var infoParts = new List<string>();

                    if (!string.IsNullOrWhiteSpace(item.Issuer))
                        infoParts.Add(item.Issuer);

                    if (item.Date.HasValue)
                        infoParts.Add(item.Date.Value.ToString("MMM yyyy"));

                    if (infoParts.Count > 0)
                    {
                        certification.Item().PaddingTop(2).Text(string.Join(" | ", infoParts))
                            .FontSize(9)
                            .FontColor(_style.MutedTextColor);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Url))
                    {
                        certification.Item().PaddingTop(3).Text(item.Url)
                            .FontSize(9)
                            .FontColor(_style.AccentColor);
                    }
                });
            }

            column.Item().PaddingBottom(6);
        }

        private void ComposeSectionTitle(ColumnDescriptor column, string title)
        {
            if (_style.Id == "classic")
            {
                column.Item().PaddingTop(4).Text(title)
                    .FontSize(14)
                    .Bold()
                    .FontColor(_style.HeaderTextColor);

                column.Item().PaddingTop(2).PaddingBottom(8)
                    .LineHorizontal(1)
                    .LineColor(_style.LineColor);

                return;
            }

            if (_style.Id == "minimal-green")
            {
                column.Item().PaddingTop(4).Text(title)
                    .FontSize(14)
                    .Bold()
                    .FontColor(_style.AccentColor);

                column.Item().PaddingTop(2).PaddingBottom(8)
                    .LineHorizontal(1)
                    .LineColor(_style.LineColor);

                return;
            }

            if (_style.Id == "warm-beige")
            {
                column.Item().PaddingTop(4).Text(title)
                    .FontSize(14)
                    .Bold()
                    .FontColor(_style.AccentColor);

                column.Item().PaddingTop(2).PaddingBottom(8)
                    .LineHorizontal(1)
                    .LineColor(_style.LineColor);

                return;
            }

            column.Item().PaddingTop(4).Text(title)
                .FontSize(14)
                .Bold()
                .FontColor(_style.AccentColor);

            column.Item().PaddingTop(2).PaddingBottom(8)
                .LineHorizontal(1)
                .LineColor(_style.LineColor);
        }

        private string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(_cv.FullName))
                return _cv.FullName;

            if (!string.IsNullOrWhiteSpace(_cv.Title))
                return _cv.Title;

            return "CV";
        }

        private string GetBlueSidebarSubtitle()
        {
            if (!string.IsNullOrWhiteSpace(_cv.Location))
                return _cv.Location;

            if (!string.IsNullOrWhiteSpace(_cv.Email))
                return _cv.Email;

            return string.Empty;
        }

        private string GetContactText()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(_cv.Email))
                parts.Add(_cv.Email);

            if (!string.IsNullOrWhiteSpace(_cv.Phone))
                parts.Add(_cv.Phone);

            if (!string.IsNullOrWhiteSpace(_cv.Location))
                parts.Add(_cv.Location);

            return string.Join(" | ", parts);
        }

        private string GetLinksText()
        {
            var links = new List<string>();

            if (!string.IsNullOrWhiteSpace(_cv.LinkedInUrl))
                links.Add($"LinkedIn: {_cv.LinkedInUrl}");

            if (!string.IsNullOrWhiteSpace(_cv.GitHubUrl))
                links.Add($"GitHub: {_cv.GitHubUrl}");

            if (!string.IsNullOrWhiteSpace(_cv.PortfolioUrl))
                links.Add($"Portfolio: {_cv.PortfolioUrl}");

            return string.Join(" | ", links);
        }

        private static string FormatDateRange(DateTime? startDate, DateTime? endDate, bool isCurrent)
        {
            if (!startDate.HasValue && !endDate.HasValue && !isCurrent)
                return string.Empty;

            var start = startDate.HasValue
                ? startDate.Value.ToString("MMM yyyy")
                : string.Empty;

            var end = isCurrent
                ? "Present"
                : endDate.HasValue
                    ? endDate.Value.ToString("MMM yyyy")
                    : string.Empty;

            if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
                return $"{start} - {end}";

            if (!string.IsNullOrWhiteSpace(start))
                return start;

            if (!string.IsNullOrWhiteSpace(end))
                return end;

            return string.Empty;
        }

        private static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "CV";

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .ToList();

            if (parts.Count == 0)
                return "CV";

            return string.Join("", parts.Select(x => x[0])).ToUpper();
        }

        private static TemplateStyle GetTemplateStyle(string? templateName)
        {
            return templateName switch
            {
                "classic" or "classic-minimal" => new TemplateStyle
                {
                    Id = "classic",
                    HeaderBackground = Colors.White,
                    HeaderTextColor = "#111827",
                    HeaderMutedTextColor = "#4B5563",
                    HeaderLinkColor = "#111827",
                    TextColor = "#111827",
                    MutedTextColor = "#4B5563",
                    AccentColor = "#111827",
                    LineColor = "#D1D5DB"
                },

                "minimal-green" or "green-professional" => new TemplateStyle
                {
                    Id = "minimal-green",
                    HeaderBackground = "#1F4D3A",
                    HeaderTextColor = Colors.White,
                    HeaderMutedTextColor = "#D1FAE5",
                    HeaderLinkColor = "#BBF7D0",
                    TextColor = "#0F172A",
                    MutedTextColor = "#64748B",
                    AccentColor = "#1F4D3A",
                    LineColor = "#BBF7D0"
                },

                "blue-sidebar" => new TemplateStyle
                {
                    Id = "blue-sidebar",
                    HeaderBackground = "#1D4ED8",
                    HeaderTextColor = Colors.White,
                    HeaderMutedTextColor = "#DBEAFE",
                    HeaderLinkColor = "#DBEAFE",
                    TextColor = "#0F172A",
                    MutedTextColor = "#64748B",
                    AccentColor = "#1D4ED8",
                    LineColor = "#CBD5E1"
                },

                "warm-beige" => new TemplateStyle
                {
                    Id = "warm-beige",
                    PageBackground = "#F2E4D8",
                    HeaderBackground = "#8B5E44",
                    HeaderTextColor = "#FFF8F1",
                    HeaderMutedTextColor = "#F3DED1",
                    HeaderLinkColor = "#F3DED1",
                    TextColor = "#40342D",
                    MutedTextColor = "#6B5A4F",
                    AccentColor = "#6B4433",
                    LineColor = "#D9BFAF"
                },

                "modern-blue" or _ => new TemplateStyle
                {
                    Id = "modern-blue",
                    HeaderBackground = "#0F172A",
                    HeaderTextColor = Colors.White,
                    HeaderMutedTextColor = "#CBD5E1",
                    HeaderLinkColor = "#93C5FD",
                    TextColor = "#0F172A",
                    MutedTextColor = "#64748B",
                    AccentColor = "#1D4ED8",
                    LineColor = "#CBD5E1"
                }
            };
        }

        private class TemplateStyle
        {
            public string Id { get; set; } = "modern-blue";

            public string PageBackground { get; set; } = "#FFFFFF";

            public string HeaderBackground { get; set; } = "#0F172A";

            public string HeaderTextColor { get; set; } = Colors.White;

            public string HeaderMutedTextColor { get; set; } = "#CBD5E1";

            public string HeaderLinkColor { get; set; } = "#93C5FD";

            public string TextColor { get; set; } = "#0F172A";

            public string MutedTextColor { get; set; } = "#64748B";

            public string AccentColor { get; set; } = "#1D4ED8";

            public string LineColor { get; set; } = "#CBD5E1";
        }
    }
}