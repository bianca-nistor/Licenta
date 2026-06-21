using System.Text.RegularExpressions;
using JobCv.Api.Dtos;

namespace JobCv.Api.Services
{
    public static class AiLanguageHelper
    {
        public static string NormalizeLanguage(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return "en";

            var value = language.Trim().ToLowerInvariant();

            return value.StartsWith("ro") ? "ro" : "en";
        }

        public static bool IsRomanian(string? language) => NormalizeLanguage(language) == "ro";

        public static string GetLanguageInstruction(string? language)
        {
            return IsRomanian(language)
                ? "Răspunde exclusiv în limba română. Toate textele vizibile pentru utilizator din JSON trebuie să fie în română. Păstrează neschimbate numele companiilor, titlurile joburilor, limbajele de programare, framework-urile, tehnologiile și acronimele când sună natural în engleză. Folosește un ton clar, profesional și util."
                : "Respond only in English. All user-facing text in the JSON must be in English. Keep company names, job titles, programming languages, frameworks, technologies and acronyms unchanged when appropriate. Use a clear, professional and helpful tone.";
        }

        public static string DefaultJobTitle(string? language) => IsRomanian(language) ? "jobul selectat" : "Selected job";
        public static string DefaultCompany(string? language) => IsRomanian(language) ? "companie necunoscută" : "Unknown company";
        public static string DefaultLocation(string? language) => IsRomanian(language) ? "nespecificată" : "Not specified";
        public static string DefaultJobDescription(string? language) => IsRomanian(language) ? "Nu există o descriere disponibilă pentru job." : "No job description available.";
        public static string DefaultCvText(string? language) => IsRomanian(language) ? "Nu a fost furnizat textul CV-ului." : "No current CV text was provided.";

        public static CvQualityCheckResponseDto LocalizeCvQualityResponse(CvQualityCheckResponseDto result, string? language)
        {
            if (!IsRomanian(language))
                return result;

            result.CompletenessLevel = TranslateQualityText(result.CompletenessLevel);
            result.Summary = TranslateQualityText(result.Summary);
            result.Strengths = result.Strengths.Select(TranslateQualityText).ToList();
            result.Suggestions = result.Suggestions.Select(TranslateQualityText).ToList();

            foreach (var issue in result.Issues)
            {
                issue.Section = TranslateQualityText(issue.Section);
                issue.Problem = TranslateQualityText(issue.Problem);
                issue.Suggestion = TranslateQualityText(issue.Suggestion);
                issue.Severity = TranslateQualityText(issue.Severity);
            }

            return result;
        }

        public static CvJobMatchResponseDto LocalizeCvJobMatchResponse(CvJobMatchResponseDto result, string? language)
        {
            if (!IsRomanian(language))
                return result;

            result.Recommendation = TranslateJobMatchText(result.Recommendation);
            result.Summary = TranslateJobMatchText(result.Summary);
            result.Strengths = result.Strengths.Select(TranslateJobMatchText).ToList();
            result.MissingSkills = result.MissingSkills.Select(TranslateJobMatchText).ToList();
            result.Improvements = result.Improvements.Select(TranslateJobMatchText).ToList();

            return result;
        }

        private static string TranslateQualityText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var text = value.Trim();

            if (_qualityTranslations.TryGetValue(text, out var translated))
                return translated;

            var summaryMatch = Regex.Match(text, @"^This CV is usable, but it still has (\d+) area\(s\) that should be improved before applying\.$");
            if (summaryMatch.Success)
                return $"Acest CV este utilizabil, dar încă are {summaryMatch.Groups[1].Value} zonă(e) care ar trebui îmbunătățite înainte de aplicare.";

            var strongMatch = Regex.Match(text, @"^This CV is strong and ready to be used for applications\. Only minor refinements may be needed\.$");
            if (strongMatch.Success)
                return "Acest CV este puternic și pregătit pentru aplicări. Pot fi necesare doar mici ajustări.";

            var weakMatch = Regex.Match(text, @"^This CV needs significant improvements before being used for applications\. It currently has (\d+) important issue\(s\)\.$");
            if (weakMatch.Success)
                return $"Acest CV are nevoie de îmbunătățiri semnificative înainte de aplicare. Momentan are {weakMatch.Groups[1].Value} problemă(e) importantă(e).";

            var incompleteMatch = Regex.Match(text, @"^This CV is incomplete\. It has (\d+) important issue\(s\), so it should be improved before being used for job applications\.$");
            if (incompleteMatch.Success)
                return $"Acest CV este incomplet. Are {incompleteMatch.Groups[1].Value} problemă(e) importantă(e), deci ar trebui îmbunătățit înainte de aplicarea la joburi.";

            return text;
        }

        private static string TranslateJobMatchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var text = value.Trim();

            if (_jobMatchTranslations.TryGetValue(text, out var translated))
                return translated;

            var scoreMatch = Regex.Match(text, @"^The score is based on overlap between important job keywords and the readable CV content\. Matched terms: (\d+)\. Missing or weak terms: (\d+)\.$");
            if (scoreMatch.Success)
            {
                return $"Scorul este calculat pe baza suprapunerii dintre cuvintele-cheie importante din job și conținutul lizibil al CV-ului. Termeni potriviți: {scoreMatch.Groups[1].Value}. Termeni lipsă sau slabi: {scoreMatch.Groups[2].Value}.";
            }

            var matchedKeyword = Regex.Match(text, @"^The CV mentions (.+), which appears relevant for this role\.$");
            if (matchedKeyword.Success)
                return $"CV-ul menționează {matchedKeyword.Groups[1].Value}, ceea ce pare relevant pentru acest rol.";

            var missingKeyword = Regex.Match(text, @"^The job description mentions (.+), but it is not clear in the CV\.$");
            if (missingKeyword.Success)
                return $"Descrierea jobului menționează {missingKeyword.Groups[1].Value}, dar acest aspect nu este clar în CV.";

            return text;
        }

        private static readonly Dictionary<string, string> _qualityTranslations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Excellent completeness"] = "Completare excelentă",
            ["Good completeness"] = "Completare bună",
            ["Medium completeness"] = "Completare medie",
            ["Low completeness"] = "Completare redusă",
            ["Profile summary"] = "Rezumat profesional",
            ["Full name"] = "Nume complet",
            ["Email"] = "Email",
            ["Phone"] = "Telefon",
            ["Skills"] = "Competențe",
            ["Experience"] = "Experiență",
            ["Education"] = "Educație",
            ["Projects"] = "Proiecte",
            ["Certifications"] = "Certificări",
            ["Languages"] = "Limbi străine",
            ["General"] = "General",
            ["High"] = "Ridicată",
            ["Medium"] = "Medie",
            ["Low"] = "Scăzută",
            ["The profile summary is missing or too short."] = "Rezumatul profesional lipsește sau este prea scurt.",
            ["Add 3-5 lines about your role, experience, technologies and career objective."] = "Adaugă 3-5 rânduri despre rolul tău, experiență, tehnologii și obiectivul profesional.",
            ["The CV does not contain a full name."] = "CV-ul nu conține numele complet.",
            ["Add your real full name in the personal information section."] = "Adaugă numele tău complet real în secțiunea de informații personale.",
            ["The CV does not contain a valid email address."] = "CV-ul nu conține o adresă de email validă.",
            ["Add a professional email address so recruiters can contact you."] = "Adaugă o adresă de email profesională, ca recrutorii să te poată contacta.",
            ["The CV does not contain a phone number."] = "CV-ul nu conține un număr de telefon.",
            ["Add a phone number if you want recruiters to contact you faster."] = "Adaugă un număr de telefon dacă vrei ca recrutorii să te contacteze mai rapid.",
            ["The CV has too few skills."] = "CV-ul are prea puține competențe.",
            ["Add at least 4-8 relevant technical and soft skills."] = "Adaugă cel puțin 4-8 competențe tehnice și soft relevante.",
            ["The CV does not contain professional experience."] = "CV-ul nu conține experiență profesională.",
            ["Add work experience, internship experience or relevant responsibilities."] = "Adaugă experiență de lucru, internship sau responsabilități relevante.",
            ["The CV does not contain education details."] = "CV-ul nu conține detalii despre educație.",
            ["Add your degree, university or relevant education program."] = "Adaugă diploma, universitatea sau programul educațional relevant.",
            ["The CV does not contain projects."] = "CV-ul nu conține proiecte.",
            ["Add at least one project with technologies, your contribution and result."] = "Adaugă cel puțin un proiect cu tehnologii, contribuția ta și rezultatul obținut.",
            ["No certifications are listed."] = "Nu sunt listate certificări.",
            ["Add certifications only if they are relevant. This is optional, but useful for junior candidates."] = "Adaugă certificări doar dacă sunt relevante. Este opțional, dar util pentru candidații juniori.",
            ["No languages are listed."] = "Nu sunt listate limbi străine.",
            ["Add languages and proficiency levels, especially English if relevant for the job market."] = "Adaugă limbi străine și niveluri de cunoaștere, mai ales engleza dacă este relevantă pentru piața muncii.",
            ["The CV appears to contain placeholder or test text."] = "CV-ul pare să conțină text temporar sau de test.",
            ["Replace generic or test values with real professional information before applying."] = "Înlocuiește valorile generice sau de test cu informații profesionale reale înainte de aplicare.",
            ["The CV includes certifications, which can support the candidate profile."] = "CV-ul include certificări, ceea ce poate susține profilul candidatului.",
            ["The CV includes language information."] = "CV-ul include informații despre limbi străine.",
            ["The profile summary is detailed enough to introduce the candidate."] = "Rezumatul profesional este suficient de detaliat pentru a prezenta candidatul.",
            ["The CV includes a useful skills section."] = "CV-ul include o secțiune utilă de competențe.",
            ["The CV includes experience information."] = "CV-ul include informații despre experiență.",
            ["The CV includes projects, which is useful especially for junior candidates."] = "CV-ul include proiecte, ceea ce este util mai ales pentru candidații juniori.",
            ["The CV contains multiple contact methods."] = "CV-ul conține mai multe metode de contact.",
            ["The CV has a basic structure that can be improved by completing the missing sections."] = "CV-ul are o structură de bază care poate fi îmbunătățită prin completarea secțiunilor lipsă.",
            ["Use concrete achievements, numbers or outcomes where possible."] = "Folosește realizări concrete, cifre sau rezultate acolo unde este posibil.",
            ["Keep descriptions specific and avoid generic phrases."] = "Păstrează descrierile specifice și evită formulările generice.",
            ["Make sure the most relevant skills for the target job are easy to find."] = "Asigură-te că cele mai relevante competențe pentru jobul vizat sunt ușor de găsit.",
            ["Review grammar, consistency and formatting before exporting the final PDF."] = "Verifică gramatica, consistența și formatarea înainte de exportul PDF final.",
            ["High completeness"] = "Completare ridicată",
            ["Very low completeness"] = "Completare foarte redusă",
            ["This CV is well structured and contains most of the information expected by recruiters."] = "Acest CV este bine structurat și conține majoritatea informațiilor așteptate de recrutori.",
            ["This CV contains too little useful information. Complete the main sections before using it for applications."] = "Acest CV conține prea puține informații utile. Completează secțiunile principale înainte de a-l folosi pentru aplicări.",
            ["Use concrete achievements instead of general statements wherever possible."] = "Folosește realizări concrete în locul afirmațiilor generale ori de câte ori este posibil.",
            ["Keep descriptions short, clear and focused on impact, technologies and responsibilities."] = "Păstrează descrierile scurte, clare și concentrate pe impact, tehnologii și responsabilități.",
            ["Add skills that match the jobs you want to apply for, such as C#, .NET, SQL, REST APIs or communication."] = "Adaugă competențe care se potrivesc joburilor la care vrei să aplici, precum C#, .NET, SQL, REST APIs sau comunicare.",
            ["Add at least one relevant project with problem, solution, technologies and your role."] = "Adaugă cel puțin un proiect relevant cu problema, soluția, tehnologiile și rolul tău.",
            ["If you do not have work experience, add internships, volunteering, university projects or freelance work."] = "Dacă nu ai experiență de lucru, adaugă internshipuri, voluntariat, proiecte universitare sau colaborări freelance.",
            ["For each project, mention the technologies used and the result of the project."] = "Pentru fiecare proiect, menționează tehnologiile folosite și rezultatul proiectului.",
        };

        private static readonly Dictionary<string, string> _jobMatchTranslations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Strong match - you can apply with this CV, but a small tailoring pass is still recommended."] = "Potrivire puternică - poți aplica folosind acest CV, dar este recomandată totuși o mică adaptare.",
            ["Good match - the CV is relevant, but tailoring the summary and skills would improve your chances."] = "Potrivire bună - CV-ul este relevant, dar adaptarea rezumatului și a competențelor ți-ar îmbunătăți șansele.",
            ["Partial match - tailor the CV before applying and add clearer evidence for the job requirements."] = "Potrivire parțială - adaptează CV-ul înainte de aplicare și adaugă dovezi mai clare pentru cerințele jobului.",
            ["Weak match - improve the CV significantly or choose a more suitable role."] = "Potrivire slabă - îmbunătățește semnificativ CV-ul sau alege un rol mai potrivit.",
            ["Strong match - apply with this CV"] = "Potrivire puternică - aplică folosind acest CV",
            ["Good match - improve the CV before applying"] = "Potrivire bună - îmbunătățește CV-ul înainte de aplicare",
            ["Partial match - tailor the CV first"] = "Potrivire parțială - adaptează mai întâi CV-ul",
            ["Weak match - improve the CV significantly first"] = "Potrivire slabă - îmbunătățește semnificativ CV-ul înainte",
            ["The CV does not clearly match the job keywords yet."] = "CV-ul nu se potrivește încă în mod clar cu termenii-cheie ai jobului.",
            ["You can still improve the match by tailoring the summary and skills sections."] = "Poți îmbunătăți potrivirea prin adaptarea secțiunilor de rezumat și competențe.",
            ["No major missing keywords were detected from the available job description."] = "Nu au fost detectați termeni-cheie importanți lipsă din descrierea disponibilă a jobului.",
            ["Rewrite the profile summary so it directly targets this job title and company."] = "Rescrie rezumatul profesional astfel încât să vizeze direct acest rol și această companie.",
            ["Move the most relevant skills near the top of the CV."] = "Mută cele mai relevante competențe aproape de începutul CV-ului.",
            ["Add project or experience descriptions that prove the required skills."] = "Adaugă descrieri de proiecte sau experiențe care demonstrează competențele cerute.",
            ["Use keywords from the job description naturally, without copying the entire text."] = "Folosește natural cuvinte-cheie din descrierea jobului, fără să copiezi întregul text.",
            ["The selected CV contains very little text. Add summary, skills, experience, projects and education before applying."] = "CV-ul selectat conține foarte puțin text. Adaugă rezumat, competențe, experiență, proiecte și educație înainte de aplicare."
        };
    }
}
