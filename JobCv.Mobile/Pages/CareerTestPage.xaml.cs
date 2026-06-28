using JobCv.Mobile.Models;
using JobCv.Mobile.Services;
using Microsoft.Maui;
using Microsoft.Maui.Storage;


namespace JobCv.Mobile.Pages
{
    public partial class CareerTestPage : ContentPage
    {
        private readonly UserDto _user;
        private readonly ApiService _apiService;

        private readonly List<CareerTestQuestion> _questions;
        private readonly Dictionary<int, bool> _answers = new();

        private int _currentIndex = 0;
        private string _language = "en";

        public CareerTestPage(UserDto user, ApiService apiService)
        {
            InitializeComponent();

            _user = user;
            _apiService = apiService;
            _language = GetCurrentLanguage();
            _questions = BuildQuestions();

            ApplyStaticLanguageText();
            ShowQuestion();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var currentLanguage = GetCurrentLanguage();
            if (_language != currentLanguage)
            {
                _language = currentLanguage;
                ApplyStaticLanguageText();
                ShowQuestion();
            }
        }

        private static string GetCurrentLanguage()
        {
            var language = Preferences.Get("AppLanguage", "en");
            return string.Equals(language, "ro", StringComparison.OrdinalIgnoreCase) ? "ro" : "en";
        }

        private bool IsRomanian => _language == "ro";

        private void ApplyStaticLanguageText()
        {
            if (IsRomanian)
            {
                TitleLabel.Text = "Test de interese profesionale";
                SubtitleLabel.Text = "Citește fiecare activitate profesională și alege dacă ți-ar plăcea să o faci. Rezultatul este calculat pe baza modelului de scorare O*NET Interest Profiler Short Form.";
                ImportantTitleLabel.Text = "Important";
                ImportantTextLabel.Text = "Răspunsurile și rezultatul nu sunt salvate automat. Dacă vrei să păstrezi rezultatul, descarcă raportul PDF la finalul testului.";
                LikeButton.Text = "Mi-ar plăcea să fac asta";
                DislikeButton.Text = "Nu mi-ar plăcea să fac asta";
                PreviousButton.Text = "Înapoi";
            }
            else
            {
                TitleLabel.Text = "Career Interest Profiler";
                SubtitleLabel.Text = "Read each work activity and choose whether you would like to do it. The result is based on the O*NET Interest Profiler Short Form scoring model.";
                ImportantTitleLabel.Text = "Important";
                ImportantTextLabel.Text = "Your answers and result are not saved automatically. If you want to keep the result, download the PDF report at the end of the test.";
                LikeButton.Text = "I would like to do this";
                DislikeButton.Text = "I would not like to do this";
                PreviousButton.Text = "Previous";
            }
        }

        private static List<CareerTestQuestion> BuildQuestions()
        {
            return new List<CareerTestQuestion>
            {
                Q(1, "R", "Realistic", "Realist", "Build kitchen cabinets", "Să construiesc dulapuri de bucătărie"),
                Q(2, "R", "Realistic", "Realist", "Drive a truck to deliver packages to offices and homes", "Să conduc un camion pentru a livra pachete la birouri și locuințe"),
                Q(3, "R", "Realistic", "Realist", "Lay brick or tile", "Să pun cărămizi sau gresie/faianță"),
                Q(4, "R", "Realistic", "Realist", "Test the quality of parts before shipment", "Să testez calitatea pieselor înainte de expediere"),
                Q(5, "R", "Realistic", "Realist", "Repair household appliances", "Să repar electrocasnice"),
                Q(6, "R", "Realistic", "Realist", "Repair and install locks", "Să repar și să instalez încuietori"),
                Q(7, "R", "Realistic", "Realist", "Raise fish in a fish hatchery", "Să cresc pești într-o crescătorie"),
                Q(8, "R", "Realistic", "Realist", "Set up and operate machines to make products", "Să configurez și să folosesc mașini pentru fabricarea produselor"),
                Q(9, "R", "Realistic", "Realist", "Assemble electronic parts", "Să asamblez componente electronice"),
                Q(10, "R", "Realistic", "Realist", "Put out forest fires", "Să sting incendii de pădure"),

                Q(11, "I", "Investigative", "Investigativ", "Develop a new medicine", "Să dezvolt un medicament nou"),
                Q(12, "I", "Investigative", "Investigativ", "Investigate the cause of a fire", "Să investighez cauza unui incendiu"),
                Q(13, "I", "Investigative", "Investigativ", "Study ways to reduce water pollution", "Să studiez metode de reducere a poluării apei"),
                Q(14, "I", "Investigative", "Investigativ", "Develop a way to better predict the weather", "Să dezvolt o metodă mai bună de prognoză a vremii"),
                Q(15, "I", "Investigative", "Investigativ", "Conduct chemical experiments", "Să realizez experimente chimice"),
                Q(16, "I", "Investigative", "Investigativ", "Work in a biology lab", "Să lucrez într-un laborator de biologie"),
                Q(17, "I", "Investigative", "Investigativ", "Study the movement of planets", "Să studiez mișcarea planetelor"),
                Q(18, "I", "Investigative", "Investigativ", "Invent a replacement for sugar", "Să inventez un înlocuitor pentru zahăr"),
                Q(19, "I", "Investigative", "Investigativ", "Examine blood samples using a microscope", "Să examinez probe de sânge la microscop"),
                Q(20, "I", "Investigative", "Investigativ", "Do laboratory tests to identify diseases", "Să fac teste de laborator pentru identificarea bolilor"),

                Q(21, "A", "Artistic", "Artistic", "Write books or plays", "Să scriu cărți sau piese de teatru"),
                Q(22, "A", "Artistic", "Artistic", "Paint sets for plays", "Să pictez decoruri pentru piese de teatru"),
                Q(23, "A", "Artistic", "Artistic", "Play a musical instrument", "Să cânt la un instrument muzical"),
                Q(24, "A", "Artistic", "Artistic", "Write scripts for movies or television shows", "Să scriu scenarii pentru filme sau emisiuni TV"),
                Q(25, "A", "Artistic", "Artistic", "Compose or arrange music", "Să compun sau să aranjez muzică"),
                Q(26, "A", "Artistic", "Artistic", "Perform jazz or tap dance", "Să dansez jazz sau step"),
                Q(27, "A", "Artistic", "Artistic", "Draw pictures", "Să desenez imagini"),
                Q(28, "A", "Artistic", "Artistic", "Sing in a band", "Să cânt într-o trupă"),
                Q(29, "A", "Artistic", "Artistic", "Create special effects for movies", "Să creez efecte speciale pentru filme"),
                Q(30, "A", "Artistic", "Artistic", "Edit movies", "Să editez filme"),

                Q(31, "S", "Social", "Social", "Teach an individual an exercise routine", "Să învăț o persoană o rutină de exerciții"),
                Q(32, "S", "Social", "Social", "Teach children how to play sports", "Să învăț copiii cum să practice sporturi"),
                Q(33, "S", "Social", "Social", "Help people with personal or emotional problems", "Să ajut persoane cu probleme personale sau emoționale"),
                Q(34, "S", "Social", "Social", "Teach sign language to people who are deaf or hard of hearing", "Să predau limbajul semnelor persoanelor surde sau cu dificultăți de auz"),
                Q(35, "S", "Social", "Social", "Give career guidance to people", "Să ofer orientare în carieră altor persoane"),
                Q(36, "S", "Social", "Social", "Help conduct a group therapy session", "Să ajut la desfășurarea unei sesiuni de terapie de grup"),
                Q(37, "S", "Social", "Social", "Perform rehabilitation therapy", "Să realizez terapie de recuperare"),
                Q(38, "S", "Social", "Social", "Take care of children at a day-care center", "Să am grijă de copii într-un centru de zi"),
                Q(39, "S", "Social", "Social", "Do volunteer work at a non-profit organization", "Să fac voluntariat într-o organizație non-profit"),
                Q(40, "S", "Social", "Social", "Teach a high-school class", "Să predau la o clasă de liceu"),

                Q(41, "E", "Enterprising", "Întreprinzător", "Buy and sell stocks and bonds", "Să cumpăr și să vând acțiuni și obligațiuni"),
                Q(42, "E", "Enterprising", "Întreprinzător", "Negotiate business contracts", "Să negociez contracte de afaceri"),
                Q(43, "E", "Enterprising", "Întreprinzător", "Manage a retail store", "Să conduc un magazin de retail"),
                Q(44, "E", "Enterprising", "Întreprinzător", "Represent a client in a lawsuit", "Să reprezint un client într-un proces"),
                Q(45, "E", "Enterprising", "Întreprinzător", "Operate a beauty salon or barber shop", "Să administrez un salon de înfrumusețare sau o frizerie"),
                Q(46, "E", "Enterprising", "Întreprinzător", "Market a new line of clothing", "Să promovez o nouă linie de îmbrăcăminte"),
                Q(47, "E", "Enterprising", "Întreprinzător", "Manage a department within a large company", "Să conduc un departament într-o companie mare"),
                Q(48, "E", "Enterprising", "Întreprinzător", "Sell merchandise at a department store", "Să vând produse într-un magazin universal"),
                Q(49, "E", "Enterprising", "Întreprinzător", "Start your own business", "Să îmi deschid propria afacere"),
                Q(50, "E", "Enterprising", "Întreprinzător", "Manage a clothing store", "Să conduc un magazin de haine"),

                Q(51, "C", "Conventional", "Convențional", "Develop a spreadsheet using computer software", "Să creez un tabel de calcul folosind un program de calculator"),
                Q(52, "C", "Conventional", "Convențional", "Calculate the wages of employees", "Să calculez salariile angajaților"),
                Q(53, "C", "Conventional", "Convențional", "Proofread records or forms", "Să verific documente sau formulare"),
                Q(54, "C", "Conventional", "Convențional", "Inventory supplies using a hand-held computer", "Să inventariez materiale folosind un dispozitiv portabil"),
                Q(55, "C", "Conventional", "Convențional", "Install software across computers on a large network", "Să instalez software pe calculatoare dintr-o rețea mare"),
                Q(56, "C", "Conventional", "Convențional", "Record rent payments", "Să înregistrez plăți de chirie"),
                Q(57, "C", "Conventional", "Convențional", "Operate a calculator", "Să folosesc un calculator de birou"),
                Q(58, "C", "Conventional", "Convențional", "Keep inventory records", "Să țin evidențe de inventar"),
                Q(59, "C", "Conventional", "Convențional", "Keep shipping and receiving records", "Să țin evidențe de expediere și recepție"),
                Q(60, "C", "Conventional", "Convențional", "Stamp, sort, and distribute mail for an organization", "Să ștampilez, sortez și distribui corespondența într-o organizație")
            };
        }

        private static CareerTestQuestion Q(
            int id,
            string code,
            string areaEn,
            string areaRo,
            string textEn,
            string textRo)
        {
            return new CareerTestQuestion
            {
                Id = id,
                AreaCode = code,
                AreaEn = areaEn,
                AreaRo = areaRo,
                Area = areaEn,
                Text = textEn,
                TextEn = textEn,
                TextRo = textRo
            };
        }

        private void ShowQuestion()
        {
            var question = _questions[_currentIndex];

            ProgressLabel.Text = IsRomanian
                ? $"Întrebarea {_currentIndex + 1} din {_questions.Count}"
                : $"Question {_currentIndex + 1} of {_questions.Count}";

            QuestionProgressBar.Progress = (double)(_currentIndex + 1) / _questions.Count;
            AreaLabel.Text = $"{question.AreaCode} - {(IsRomanian ? question.AreaRo : question.AreaEn)}";
            QuestionLabel.Text = IsRomanian ? question.TextRo : question.TextEn;

            NextButton.Text = _currentIndex == _questions.Count - 1
                ? (IsRomanian ? "Vezi rezultatul" : "See result")
                : (IsRomanian ? "Următorul" : "Next");

            MessageLabel.IsVisible = false;
            MessageLabel.Text = "";

            ResetAnswerButtonStyles();

            if (_answers.TryGetValue(question.Id, out var selectedValue))
                HighlightAnswer(selectedValue);
        }

        private void OnAnswerClicked(object sender, EventArgs e)
        {
            if ((sender as Button)?.CommandParameter is not string valueText)
                return;

            if (!bool.TryParse(valueText, out var value))
                return;

            var question = _questions[_currentIndex];

            _answers[question.Id] = value;

            ResetAnswerButtonStyles();
            HighlightAnswer(value);

            MessageLabel.IsVisible = false;
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            var question = _questions[_currentIndex];

            if (!_answers.ContainsKey(question.Id))
            {
                MessageLabel.Text = IsRomanian
                    ? "Te rog selectează un răspuns înainte de a continua."
                    : "Please select an answer before continuing.";
                MessageLabel.IsVisible = true;
                return;
            }

            if (_currentIndex < _questions.Count - 1)
            {
                _currentIndex++;
                ShowQuestion();
                return;
            }

            var result = BuildResult();

            await Navigation.PushAsync(new CareerTestResultPage(_user, _apiService, result));
        }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            if (_currentIndex == 0)
                return;

            _currentIndex--;
            ShowQuestion();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                IsRomanian ? "Ieși din test?" : "Leave test?",
                IsRomanian
                    ? "Răspunsurile nu sunt salvate. Dacă ieși acum, va trebui să reiei testul de la început."
                    : "Your answers are not saved. If you leave now, you will need to start the test again.",
                IsRomanian ? "Ieși" : "Leave",
                IsRomanian ? "Rămâi" : "Stay");

            if (!confirm)
                return;

            await Navigation.PopAsync();
        }

        private CareerTestResult BuildResult()
        {
            var groupedScores = _questions
                .GroupBy(q => q.AreaCode)
                .Select(group =>
                {
                    var rawScore = group.Count(q => _answers.TryGetValue(q.Id, out var liked) && liked);
                    var maxScore = group.Count();
                    var percent = maxScore == 0 ? 0 : (int)Math.Round((double)rawScore / maxScore * 100);
                    var code = group.Key;

                    return new CareerAreaScore
                    {
                        AreaCode = code,
                        Area = GetAreaName(code),
                        Score = percent,
                        RawScore = rawScore,
                        MaxScore = maxScore,
                        Description = GetAreaDescription(code)
                    };
                })
                .OrderByDescending(x => x.RawScore)
                .ThenBy(x => GetRiasecOrder(x.AreaCode))
                .ToList();

            var topAreas = groupedScores.Take(3).ToList();
            var profileCode = string.Join("-", topAreas.Select(x => x.AreaCode));

            return new CareerTestResult
            {
                ProfileTitle = IsRomanian ? $"Profil RIASEC: {profileCode}" : $"RIASEC profile: {profileCode}",
                ProfileCode = profileCode,
                Language = _language,
                Summary = BuildSummary(topAreas),
                SourceAttribution = BuildSourceAttribution(),
                GeneratedAt = DateTime.Now,
                AreaScores = groupedScores,
                RecommendedRoles = new List<string>(),
                NextSteps = BuildNextSteps(topAreas)
            };
        }

        private static int GetRiasecOrder(string code)
        {
            return code switch
            {
                "R" => 0,
                "I" => 1,
                "A" => 2,
                "S" => 3,
                "E" => 4,
                "C" => 5,
                _ => 99
            };
        }

        private string GetAreaName(string code)
        {
            return code switch
            {
                "R" => IsRomanian ? "Realist" : "Realistic",
                "I" => IsRomanian ? "Investigativ" : "Investigative",
                "A" => IsRomanian ? "Artistic" : "Artistic",
                "S" => IsRomanian ? "Social" : "Social",
                "E" => IsRomanian ? "Întreprinzător" : "Enterprising",
                "C" => IsRomanian ? "Convențional" : "Conventional",
                _ => code
            };
        }

        private string BuildSummary(List<CareerAreaScore> topAreas)
        {
            var names = topAreas.Select(x => $"{x.AreaCode} - {x.Area}").ToList();

            if (IsRomanian)
            {
                return
                    $"Cele mai ridicate arii de interes sunt {string.Join(", ", names)}. " +
                    "Scorurile sunt calculate prin numărarea activităților selectate în fiecare categorie RIASEC, conform modelului O*NET Interest Profiler Short Form Paper-and-Pencil. " +
                    "Rezultatul este orientativ și ajută la explorarea direcțiilor profesionale, nu reprezintă un diagnostic psihologic.";
            }

            return
                $"Your highest interest areas are {string.Join(", ", names)}. " +
                "Scores are calculated by counting the activities selected in each RIASEC category, following the O*NET Interest Profiler Short Form Paper-and-Pencil scoring model. " +
                "This result is an orientation tool for career exploration, not a psychological diagnosis.";
        }

        private List<string> BuildRecommendedRoles(List<CareerAreaScore> topAreas)
        {
            var codes = topAreas.Select(x => x.AreaCode).ToHashSet();
            var roles = new List<string>();

            if (codes.Contains("R"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "meserii tehnice și practice", "operațiuni/logistică", "mentenanță și reparații", "tehnician în domenii aplicate" }
                    : new[] { "skilled trades and practical technical work", "operations/logistics", "maintenance and repair", "applied technician roles" });
            }

            if (codes.Contains("I"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "cercetare și analiză", "laborator sau științe aplicate", "analiză de date", "roluri tehnice bazate pe investigație" }
                    : new[] { "research and analysis", "laboratory or applied science", "data analysis", "investigative technical roles" });
            }

            if (codes.Contains("A"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "design și creație vizuală", "scriere sau conținut digital", "media și producție", "UX/UI sau comunicare creativă" }
                    : new[] { "design and visual creation", "writing or digital content", "media and production", "UX/UI or creative communication" });
            }

            if (codes.Contains("S"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "educație și training", "resurse umane", "consiliere sau suport social", "customer success și relații cu clienții" }
                    : new[] { "education and training", "human resources", "counseling or social support", "customer success and client-facing roles" });
            }

            if (codes.Contains("E"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "vânzări și negociere", "antreprenoriat", "coordonare de proiect", "management sau business development" }
                    : new[] { "sales and negotiation", "entrepreneurship", "project coordination", "management or business development" });
            }

            if (codes.Contains("C"))
            {
                roles.AddRange(IsRomanian
                    ? new[] { "administrativ și organizare", "contabilitate/payroll", "raportare și documentație", "operațiuni de birou" }
                    : new[] { "administration and organization", "accounting/payroll", "reporting and documentation", "office operations" });
            }

            return roles.Distinct().Take(8).ToList();
        }

        private List<string> BuildNextSteps(List<CareerAreaScore> topAreas)
        {
            var code = string.Join("-", topAreas.Select(x => x.AreaCode));

            if (IsRomanian)
            {
                return new List<string>
                {
                    $"Notează profilul RIASEC obținut: {code}.",
                    "Caută ocupații sau domenii care se potrivesc cu primele 2-3 arii de interes.",
                    "Compară direcțiile recomandate cu experiența, educația și competențele tale actuale.",
                    "Alege 2-3 roluri care par interesante și caută descrieri reale de joburi pentru ele.",
                    "Actualizează CV-ul în aplicație pentru direcția profesională pe care vrei să o explorezi."
                };
            }

            return new List<string>
            {
                $"Write down your RIASEC profile code: {code}.",
                "Explore occupations or domains that match your top 2-3 interest areas.",
                "Compare the suggested directions with your current education, experience and skills.",
                "Choose 2-3 roles that seem interesting and review real job descriptions for them.",
                "Update your CV in the app for the career direction you want to explore."
            };
        }

        private string GetAreaDescription(string code)
        {
            return code switch
            {
                "R" => IsRomanian
                    ? "Interes pentru activități practice, lucru cu obiecte, unelte, echipamente sau activități tehnice concrete."
                    : "Interest in practical activities, working with objects, tools, equipment or hands-on technical tasks.",
                "I" => IsRomanian
                    ? "Interes pentru cercetare, analiză, experimente, rezolvare de probleme și înțelegerea cauzelor."
                    : "Interest in research, analysis, experiments, problem solving and understanding causes.",
                "A" => IsRomanian
                    ? "Interes pentru creație, expresie, artă, design, scriere, muzică sau activități imaginative."
                    : "Interest in creation, expression, art, design, writing, music or imaginative activities.",
                "S" => IsRomanian
                    ? "Interes pentru lucru cu oameni, ajutor, predare, consiliere, îngrijire sau sprijin."
                    : "Interest in working with people, helping, teaching, counseling, care or support.",
                "E" => IsRomanian
                    ? "Interes pentru persuasiune, conducere, vânzări, afaceri, organizare și luarea deciziilor."
                    : "Interest in persuasion, leadership, sales, business, organization and decision making.",
                "C" => IsRomanian
                    ? "Interes pentru organizare, date, documente, proceduri, evidențe și activități structurate."
                    : "Interest in organization, data, documents, procedures, records and structured activities.",
                _ => string.Empty
            };
        }

        private string BuildSourceAttribution()
        {
            if (IsRomanian)
            {
                return "Bazat pe O*NET Interest Profiler Short Form Paper-and-Pencil Version, sponsorizat de U.S. Department of Labor, Employment & Training Administration și dezvoltat de National Center for O*NET Development. Traducerea/adaptarea în limba română este realizată în scop educațional pentru aplicația Career Guide și nu este aprobată, testată sau validată de U.S. Department of Labor / Employment and Training Administration.";
            }

            return "Based on the O*NET Interest Profiler Short Form Paper-and-Pencil Version, sponsored by the U.S. Department of Labor, Employment & Training Administration and developed by the National Center for O*NET Development. The Romanian translation/adaptation is provided for the Career Guide educational application and is not approved, tested, or validated by the U.S. Department of Labor / Employment and Training Administration.";
        }

        private void ResetAnswerButtonStyles()
        {
            var buttons = new[]
            {
                LikeButton,
                DislikeButton
            };

            foreach (var button in buttons)
            {
                button.BackgroundColor = Color.FromArgb("#F8FAFC");
                button.TextColor = Color.FromArgb("#0F172A");
                button.BorderColor = Color.FromArgb("#CBD5E1");
            }
        }

        private void HighlightAnswer(bool liked)
        {
            var button = liked ? LikeButton : DislikeButton;

            button.BackgroundColor = Color.FromArgb("#1D4ED8");
            button.TextColor = Colors.White;
            button.BorderColor = Color.FromArgb("#1D4ED8");
        }
    }
}
