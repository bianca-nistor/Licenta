using System.ComponentModel;
using System.Text.RegularExpressions;

namespace JobCv.Mobile.Services
{
    public static class UiTranslationService
    {
        private static readonly BindableProperty IsWatchingProperty = BindableProperty.CreateAttached(
            "IsWatchingForLanguageChanges",
            typeof(bool),
            typeof(UiTranslationService),
            false);

        private static readonly BindableProperty IsUpdatingProperty = BindableProperty.CreateAttached(
            "IsUpdatingLanguageText",
            typeof(bool),
            typeof(UiTranslationService),
            false);

        private static readonly HashSet<string> DoNotTranslate = new(StringComparer.OrdinalIgnoreCase)
        {
            "Career Guide",
            "Ghid de carieră"
        };

        private static readonly Dictionary<string, string> EnglishToRomanian = new(StringComparer.Ordinal)
        {
            // Navigation / common
            ["Home"] = "Acasă",
            ["Dashboard"] = "Dashboard",
            ["Career Dashboard"] = "Dashboard carieră",
            ["View profile"] = "Vezi profilul",
            ["Menu"] = "Meniu",
            ["Applications"] = "Aplicări",
            ["Jobs"] = "Joburi",
            ["Logout"] = "Deconectare",
            ["Cancel"] = "Anulează",
            ["Delete"] = "Șterge",
            ["Delete file"] = "Șterge fișierul",

            ["Edit"] = "Editează",
            ["Save changes"] = "Salvează modificările",
            ["Save"] = "Salvează",
            ["Open"] = "Deschide",
            ["Preview"] = "Previzualizare",
            ["Export"] = "Exportă",
            ["Details"] = "Detalii",
            ["Description"] = "Descriere",
            ["Optional"] = "Opțional",
            ["Choose"] = "Alege",
            ["Start"] = "Începe",
            ["Upload"] = "Încarcă",
            ["Extract"] = "Extrage",
            ["Extract information"] = "Extrage informațiile",
            ["+ Add"] = "+ Adaugă",
            ["Search"] = "Caută",
            ["Filter"] = "Filtru",
            ["Filters"] = "Filtre",
            ["Sort"] = "Sortare",
            ["All"] = "Toate",
            ["Saved"] = "Salvat",
            ["Applied"] = "Aplicat",
            ["Rejected"] = "Respins",
            ["Offer"] = "Ofertă",
            ["Interview"] = "Interviu",
            ["Interview Scheduled"] = "Interviu programat",
            ["Interview Done"] = "Interviu finalizat",
            ["Yes"] = "Da",
            ["No"] = "Nu",
            ["OK"] = "OK",
            ["Error"] = "Eroare",
            ["Loading preview..."] = "Se încarcă previzualizarea...",
            ["Loading PDF preview..."] = "Se încarcă previzualizarea PDF...",
            ["Not set"] = "Nesetat",
            ["Not provided"] = "Nefurnizat",
            ["Not specified"] = "Nespecificat",
            ["Not available"] = "Indisponibil",
            ["No file selected"] = "Niciun fișier selectat",
            ["Selected file"] = "Fișier selectat",
            ["No CV selected"] = "Niciun CV selectat",
            ["No contact information added"] = "Nu au fost adăugate date de contact",
            ["No summary added yet."] = "Nu a fost adăugat încă un rezumat.",
            ["No professional summary added yet."] = "Nu a fost adăugat încă un rezumat profesional.",
            ["No name added"] = "Nu a fost adăugat numele",
            ["Email not available"] = "Email indisponibil",
            ["Location not specified"] = "Locație nespecificată",
            ["Location not available"] = "Locație indisponibilă",
            ["Unknown company"] = "Companie necunoscută",
            ["Company not available"] = "Companie indisponibilă",
            ["Untitled job"] = "Job fără titlu",
            ["Untitled CV"] = "CV fără titlu",
            ["Untitled item"] = "Element fără titlu",

            // Login / register
            ["Build better CVs, track applications and prepare for interviews."] = "Creează CV-uri mai bune, urmărește aplicările și pregătește-te pentru interviuri.",
            ["Welcome back"] = "Bine ai revenit",
            ["Sign in to continue your career progress."] = "Autentifică-te pentru a-ți continua progresul în carieră.",
            ["Email"] = "Email",
            ["Password"] = "Parolă",
            ["Enter your email"] = "Introdu emailul",
            ["Enter your password"] = "Introdu parola",
            ["Sign In"] = "Autentificare",
            ["Create new account"] = "Creează cont nou",
            ["Your personal assistant for CVs, jobs, applications and interviews."] = "Asistentul tău personal pentru CV-uri, joburi, aplicări și interviuri.",
            ["Start building your career profile."] = "Începe să-ți construiești profilul de carieră.",
            ["Join Career Guide"] = "Alătură-te Career Guide",
            ["Create your account and manage your CVs, jobs and applications."] = "Creează-ți contul și gestionează CV-urile, joburile și aplicările.",
            ["Create account"] = "Creează cont",
            ["Confirm password"] = "Confirmă parola",
            ["Back to sign in"] = "Înapoi la autentificare",
            ["Enter your full name"] = "Introdu numele complet",
            ["Create a password"] = "Creează o parolă",
            ["Confirm your password"] = "Confirmă parola",

            // Profile
            ["Manage your account and career preferences."] = "Gestionează contul și preferințele de carieră.",
            ["JobCv user"] = "Utilizator JobCv",
            ["Career level"] = "Nivel carieră",
            ["Preferred job type"] = "Tip job preferat",
            ["Edit profile"] = "Editează profilul",
            ["Contact details"] = "Date de contact",
            ["Phone number"] = "Număr de telefon",
            ["Alternative email"] = "Email alternativ",
            ["Location"] = "Locație",
            ["Quick actions"] = "Acțiuni rapide",
            ["Session"] = "Sesiune",
            ["You can safely log out from your account."] = "Te poți deconecta în siguranță din cont.",
            ["Language"] = "Limbă",
            ["CV language"] = "Limba CV-ului",
            ["Romanian"] = "Română",
            ["App language"] = "Limba aplicației",
            ["Choose the app language. Job listings may still appear in their original language."] = "Alege limba aplicației. Anunțurile de job pot apărea în continuare în limba originală.",
            ["English"] = "Engleză",
            ["Română"] = "Română",
            ["Current language: English"] = "Limba curentă: Engleză",
            ["Current language: Romanian"] = "Limba curentă: Română",
            ["Are you sure you want to log out?"] = "Sigur vrei să te deconectezi?",

            // CV creation/import/upload
            ["Create CV"] = "Creează CV",
            ["Choose how you want to start your CV."] = "Alege cum vrei să începi CV-ul.",
            ["Start from scratch"] = "Începe de la zero",
            ["Create a new CV and add your personal information, photo, skills, education, experience and projects."] = "Creează un CV nou și adaugă informațiile personale, fotografia, competențele, educația, experiența și proiectele.",
            ["Duplicate existing CV"] = "Copiază un CV existent",
            ["Create a new version based on one of your existing CVs."] = "Creează o versiune nouă pe baza unuia dintre CV-urile tale existente.",
            ["Start from your CV"] = "Pornește de la CV-ul tău",
            ["Upload a PDF CV and we’ll copy the information we can find into a new editable CV."] = "Încarcă un CV PDF și vom copia informațiile găsite într-un CV nou editabil.",
            ["Upload existing CV"] = "Încarcă un CV existent",
            ["Upload an existing PDF, DOC or DOCX CV."] = "Încarcă un CV existent în format PDF, DOC sau DOCX.",
            ["Upload an existing PDF, DOC or DOCX CV and keep it in your CV library."] = "Încarcă un CV existent în format PDF, DOC sau DOCX și păstrează-l în biblioteca ta.",
            ["Upload an existing PDF CV and keep it in your CV library."] = "Încarcă un CV PDF existent și păstrează-l în biblioteca ta.",
            ["Choose your PDF"] = "Alege PDF-ul",
            ["Choose your PDF CV"] = "Alege CV-ul PDF",
            ["For best results, use a PDF exported from this app. Other PDFs may import only basic information."] = "Pentru cele mai bune rezultate, folosește un PDF exportat din aplicație. Alte PDF-uri pot importa doar informații de bază.",
            ["New CV title"] = "Titlul noului CV",
            ["Imported CV"] = "CV importat",
            ["Choose PDF"] = "Alege PDF",
            ["Choose your CV file"] = "Alege fișierul CV",
            ["Choose your file"] = "Alege fișierul",
            ["Choose file"] = "Alege fișier",
            ["Accepted formats: PDF, DOC, DOCX. Maximum size: 10 MB."] = "Formate acceptate: PDF, DOC, DOCX. Dimensiune maximă: 10 MB.",
            ["Upload CV"] = "Încarcă CV",
            ["Uploaded CV files"] = "Fișiere CV încărcate",
            ["No uploaded files yet"] = "Nu există fișiere încărcate încă",
            ["Upload your first existing CV file."] = "Încarcă primul tău fișier CV existent.",
            ["PDF preview"] = "Previzualizare PDF",
            ["Preview your PDF file inside the app."] = "Previzualizează fișierul PDF în aplicație.",

            // CV details/edit
            ["CV Details"] = "Detalii CV",
            ["Preview and manage your CV."] = "Previzualizează și gestionează CV-ul.",
            ["Preview and manage this CV version."] = "Previzualizează și gestionează această versiune de CV.",
            ["Export PDF"] = "Exportă PDF",
            ["Links"] = "Linkuri",
            ["Summary"] = "Rezumat",
            ["Template"] = "Șablon",
            ["Base CV"] = "CV de bază",
            ["Next sections"] = "Secțiunile următoare",
            ["Skills, education, experience, projects, languages and certifications will be editable from this page."] = "Competențele, educația, experiența, proiectele, limbile și certificările vor putea fi editate din această pagină.",
            ["Edit CV"] = "Editează CV",
            ["Update your CV information and sections."] = "Actualizează informațiile și secțiunile CV-ului.",
            ["CV photo"] = "Fotografie CV",
            ["Optional. Add a professional photo if you want it to appear in the CV header."] = "Opțional. Adaugă o fotografie profesională dacă vrei să apară în antetul CV-ului.",
            ["Choose photo"] = "Alege fotografia",
            ["Basic information"] = "Informații de bază",
            ["Save basic information"] = "Salvează informațiile de bază",
            ["Save CV"] = "Salvează CV-ul",
            ["CV title"] = "Titlu CV",
            ["Full name"] = "Nume complet",
            ["Full Name"] = "Nume complet",
            ["Email • Phone • Location"] = "Email • Telefon • Locație",
            ["Phone"] = "Telefon",
            ["LinkedIn URL"] = "URL LinkedIn",
            ["GitHub URL"] = "URL GitHub",
            ["Portfolio URL"] = "URL portofoliu",
            ["Professional summary"] = "Rezumat profesional",
            ["Skills"] = "Competențe",
            ["Education"] = "Educație",
            ["Experience"] = "Experiență",
            ["Projects"] = "Proiecte",
            ["Languages"] = "Limbi",
            ["Certifications"] = "Certificări",
            ["Add skill"] = "Adaugă competență",
            ["Edit skill"] = "Editează competența",
            ["Delete skill"] = "Șterge competența",
            ["Add language"] = "Adaugă limbă",
            ["Edit language"] = "Editează limba",
            ["Delete language"] = "Șterge limba",
            ["Add Education"] = "Adaugă educație",
            ["Add education details to your CV."] = "Adaugă detalii despre educație în CV.",
            ["Institution"] = "Instituție",
            ["Degree"] = "Diplomă / specializare",
            ["Save education"] = "Salvează educația",
            ["Add Experience"] = "Adaugă experiență",
            ["Add work experience details to your CV."] = "Adaugă detalii despre experiența profesională în CV.",
            ["I currently work here"] = "Lucrez aici în prezent",
            ["Save experience"] = "Salvează experiența",
            ["Add Project"] = "Adaugă proiect",
            ["Add a project, portfolio item or academic work."] = "Adaugă un proiect, un element de portofoliu sau o lucrare academică.",
            ["Project title"] = "Titlu proiect",
            ["Technologies"] = "Tehnologii",
            ["Project URL"] = "URL proiect",
            ["Save project"] = "Salvează proiectul",
            ["Add Certification"] = "Adaugă certificare",
            ["Add courses, certificates or professional credentials."] = "Adaugă cursuri, certificate sau acreditări profesionale.",
            ["Certification name"] = "Nume certificare",
            ["Issuer"] = "Emitent",
            ["Certificate URL"] = "URL certificat",
            ["Save certification"] = "Salvează certificarea",
            ["Add start date"] = "Adaugă data de început",
            ["Add end date"] = "Adaugă data de final",
            ["Add certification date"] = "Adaugă data certificării",

            // My CVs / dashboard
            ["View, edit, preview and manage all your created and uploaded CVs."] = "Vizualizează, editează, previzualizează și gestionează toate CV-urile create și încărcate.",
            ["Organize CVs"] = "Organizează CV-urile",
            ["Created CVs"] = "CV-uri create",
            ["Newest"] = "Cele mai noi",
            ["Oldest"] = "Cele mai vechi",
            ["CVs built and edited inside the application."] = "CV-uri construite și editate în aplicație.",
            ["No created CVs yet"] = "Nu există CV-uri create încă",
            ["Create your first structured CV to edit sections and export it later."] = "Creează primul CV structurat pentru a-l edita și exporta mai târziu.",
            ["Create your first CV"] = "Creează primul CV",
            ["Quality"] = "Calitate",
            ["Create another CV"] = "Creează alt CV",
            ["Uploaded CVs"] = "CV-uri încărcate",
            ["Files uploaded by you. These can be used later when applying to jobs."] = "Fișiere încărcate de tine. Pot fi folosite mai târziu la aplicări.",
            ["No uploaded CVs yet"] = "Nu există CV-uri încărcate încă",
            ["uploaded"] = "încărcat",
            ["Open file"] = "Deschide fișierul",
            ["Upload another CV"] = "Încarcă alt CV",
            ["Search CV by title"] = "Caută CV după titlu",
            ["Activity by period"] = "Activitate pe perioadă",
            ["Last 7 days"] = "Ultimele 7 zile",
            ["Last 30 days"] = "Ultimele 30 de zile",
            ["Last 90 days"] = "Ultimele 90 de zile",
            ["All time"] = "Toată perioada",
            ["Showing activity for last 7 days."] = "Se afișează activitatea pentru ultimele 7 zile.",
            ["Showing activity for last 30 days."] = "Se afișează activitatea pentru ultimele 30 de zile.",
            ["Showing activity for last 90 days."] = "Se afișează activitatea pentru ultimele 90 de zile.",
            ["Showing activity for all time."] = "Se afișează activitatea pentru toată perioada.",
            ["Dashboard data could not be loaded."] = "Datele din dashboard nu au putut fi încărcate.",
            ["No insight available right now."] = "Nu există momentan o observație disponibilă.",
            ["You have no saved or tracked applications yet. Start by searching jobs and saving the best opportunities."] = "Nu ai încă aplicări salvate sau urmărite. Începe prin a căuta joburi și a salva cele mai bune oportunități.",
            ["Great progress. You have 1 offer(s). Review your applications and keep tracking the next steps."] = "Progres bun. Ai 1 ofertă. Revizuiește aplicările și urmărește pașii următori.",
            ["Most of your opportunities are still saved. Choose the best matches and move them to Applied when you send your CV."] = "Majoritatea oportunităților sunt încă salvate. Alege cele mai potrivite și mută-le la Aplicat când trimiți CV-ul.",
            ["You have several rejected applications. Review your CV Quality Check and use AI Match Score before applying again."] = "Ai mai multe aplicări respinse. Revizuiește verificarea calității CV-ului și folosește scorul de potrivire înainte să aplici din nou.",
            ["Your activity is balanced. Keep creating targeted CVs and tracking each application."] = "Activitatea ta este echilibrată. Continuă să creezi CV-uri țintite și să urmărești fiecare aplicare.",
            ["Choose a period to analyze your activity."] = "Alege o perioadă pentru analiza activității.",
            ["CVs created"] = "CV-uri create",
            ["CVs uploaded"] = "CV-uri încărcate",
            ["Applications added"] = "Aplicări adăugate",
            ["Interviews"] = "Interviuri",
            ["Application pipeline"] = "Fluxul aplicărilor",
            ["See where your applications are in the recruitment process."] = "Vezi unde se află aplicările tale în procesul de recrutare.",
            ["Offers"] = "Oferte",
            ["Quick insight"] = "Observație rapidă",
            ["Your dashboard insights will appear here."] = "Observațiile pentru dashboard vor apărea aici.",
            ["Prepare your next application"] = "Pregătește următoarea aplicare",
            ["Create tailored CVs, search job opportunities and track your progress."] = "Creează CV-uri adaptate, caută oportunități și urmărește progresul.",
            ["Create new CV"] = "Creează CV nou",
            ["My applications"] = "Aplicările mele",
            ["Recent applications"] = "Aplicări recente",
            ["Your latest tracked job applications."] = "Cele mai recente aplicări la joburi urmărite.",
            ["View all"] = "Vezi toate",
            ["No applications tracked yet. Search jobs and save your first application."] = "Nu ai aplicări urmărite încă. Caută joburi și salvează prima aplicare.",

            // Applications
            ["Application Details"] = "Detalii aplicare",
            ["Edit the status, notes and interview details for this application."] = "Editează statusul, notițele și detaliile interviului pentru această aplicare.",
            ["Application progress"] = "Progres aplicare",
            ["Job information"] = "Informații job",
            ["Job title"] = "Titlu job",
            ["Company"] = "Companie",
            ["Source"] = "Sursă",
            ["Status"] = "Status",
            ["CV used"] = "CV folosit",
            ["Applied date"] = "Data aplicării",
            ["Interview date"] = "Data interviului",
            ["Notes"] = "Notițe",
            ["Job URL"] = "URL job",
            ["Choose status"] = "Alege statusul",
            ["AI cover letter"] = "Scrisoare de intenție AI",
            ["Generate a short cover letter for this application using the selected CV and job details."] = "Generează o scrisoare scurtă de intenție pentru această aplicare, folosind CV-ul selectat și detaliile jobului.",
            ["Generate cover letter"] = "Generează scrisoarea",
            ["Copy cover letter"] = "Copiază scrisoarea",
            ["Open job"] = "Deschide jobul",
            ["Prepare interview"] = "Pregătire interviu",
            ["Track application"] = "Urmărește aplicarea",
            ["Save this job and keep notes about your application process."] = "Salvează acest job și păstrează notițe despre procesul de aplicare.",
            ["Application details"] = "Detalii aplicare",
            ["Add applied date"] = "Adaugă data aplicării",
            ["Add interview date"] = "Adaugă data interviului",
            ["Save application"] = "Salvează aplicarea",
            ["Salary range optional"] = "Interval salarial opțional",
            ["Contact person optional"] = "Persoană de contact opțională",
            ["General notes about this job or application..."] = "Notițe generale despre acest job sau aplicare...",
            ["Interview notes, questions, feedback..."] = "Notițe de interviu, întrebări, feedback...",
            ["My Applications"] = "Aplicările mele",
            ["Track saved jobs, applications and interview progress."] = "Urmărește joburile salvate, aplicările și progresul interviurilor.",
            ["Applications overview"] = "Prezentare aplicări",
            ["Showing all applications."] = "Se afișează toate aplicările.",
            ["No applications found"] = "Nu au fost găsite aplicări",
            ["You do not have applications for this filter yet."] = "Nu ai încă aplicări pentru acest filtru.",
            ["Search by job title"] = "Caută după titlul jobului",

            // Jobs
            ["Find jobs"] = "Găsește joburi",
            ["Find Jobs"] = "Găsește joburi",
            ["Search jobs"] = "Caută joburi",
            ["Search jobs, filter results and open details before applying."] = "Caută joburi, filtrează rezultatele și deschide detaliile înainte de aplicare.",
            ["Enter a role and location, then refine results using filters."] = "Introdu un rol și o locație, apoi rafinează rezultatele folosind filtre.",
            ["Keyword"] = "Cuvânt-cheie",
            ["Work mode"] = "Mod de lucru",
            ["Remote"] = "La distanță",
            ["Hybrid"] = "Hibrid",
            ["On-site"] = "La birou",
            ["Any"] = "Oricare",
            ["With salary"] = "Cu salariu",
            ["No salary specified"] = "Fără salariu specificat",
            ["Default"] = "Implicit",
            ["Salary available first"] = "Salariile disponibile primele",
            ["Company A-Z"] = "Companie A-Z",
            ["Title A-Z"] = "Titlu A-Z",
            ["Source"] = "Sursă",
            ["Apply filters"] = "Aplică filtrele",
            ["Reset filters"] = "Resetează filtrele",
            ["No jobs loaded yet"] = "Nu au fost încărcate joburi încă",
            ["Search for a role to see job offers here."] = "Caută un rol pentru a vedea oferte de job aici.",
            ["Filters are applied after a search."] = "Filtrele se aplică după o căutare.",
            ["View details"] = "Vezi detalii",
            ["Load more jobs"] = "Încarcă mai multe joburi",
            ["e.g. .NET developer"] = "ex. .NET developer",
            ["e.g. Bucharest"] = "ex. București",
            ["Job Details"] = "Detalii job",
            ["Review the job, prepare your CV and track your application."] = "Revizuiește jobul, pregătește CV-ul și urmărește aplicarea.",
            ["View full job post"] = "Vezi anunțul complet",
            ["Actions"] = "Acțiuni",
            ["Choose what you want to do with this job."] = "Alege ce vrei să faci cu acest job.",
            ["Apply"] = "Aplică",
            ["Save Job"] = "Salvează jobul",
            ["Track Application"] = "Urmărește aplicarea",
            ["Posted"] = "Publicat",
            ["Salary"] = "Salariu",
            ["Salary range"] = "Interval salarial",
            ["Recruiter / contact person"] = "Recrutor / persoană de contact",
            ["No jobs were found for this search."] = "Nu au fost găsite joburi pentru această căutare.",
            ["No jobs match the selected filters."] = "Niciun job nu corespunde filtrelor selectate.",

            // AI / assistance pages
            ["AI Match Score"] = "Scor potrivire AI",
            ["AI Match"] = "Potrivire AI",
            ["Compare one of your CVs with the selected job and get an explainable score."] = "Compară unul dintre CV-urile tale cu jobul selectat și primește un scor explicabil.",
            ["Selected job"] = "Job selectat",
            ["Job description"] = "Descriere job",
            ["Choose CV"] = "Alege CV",
            ["Paste CV text"] = "Lipește textul CV-ului",
            ["Use manual CV text instead"] = "Folosește text CV introdus manual",
            ["Generate match score"] = "Generează scorul de potrivire",
            ["Recommendation"] = "Recomandare",
            ["Strengths"] = "Puncte forte",
            ["Missing / weak areas"] = "Zone lipsă / slabe",
            ["What to improve"] = "Ce trebuie îmbunătățit",
            ["Paste your CV text here..."] = "Lipește textul CV-ului aici...",
            ["CV quality"] = "Calitate CV",
            ["The CV quality check could not be generated."] = "Verificarea calității CV-ului nu a putut fi generată.",
            ["Created CV"] = "CV creat",
            ["Created inside the app"] = "CV creat în aplicație",
            ["Uploaded CV"] = "CV încărcat",
            ["Uploaded CV file."] = "Fișier CV încărcat.",
            ["Write or paste CV text manually."] = "Scrie sau lipește manual textul CV-ului.",
            ["Match score generated successfully."] = "Scorul de potrivire a fost generat cu succes.",
            ["Match score generated with explainable demo logic."] = "Scorul de potrivire a fost generat cu logică demonstrativă explicabilă.",
            ["Generated with AI."] = "Generat cu AI.",
            ["Interview preparation generated successfully."] = "Pregătirea pentru interviu a fost generată cu succes.",
            ["Demo AI result: this is a mock response. Later it can be connected to OpenAI or a local LLM."] = "Rezultat demonstrativ: răspuns generat local pentru testare.",
            ["Loading application..."] = "Se încarcă aplicarea...",
            ["The application could not be loaded."] = "Aplicarea nu a putut fi încărcată.",
            ["Subject"] = "Subiect",
            ["Cover letter generated with AI."] = "Scrisoarea de intenție a fost generată cu AI.",
            ["Demo cover letter generated."] = "Scrisoare de intenție generată demonstrativ.",
            ["Cover letter copied to clipboard."] = "Scrisoarea de intenție a fost copiată în clipboard.",
            ["No CVs available"] = "Nu există CV-uri disponibile",
            ["CV Quality Check"] = "Verificare calitate CV",
            ["Check how complete and professional this CV is."] = "Verifică cât de complet și profesional este acest CV.",
            ["Not checked yet"] = "Nu a fost verificat încă",
            ["Press the button below to analyze the selected CV."] = "Apasă butonul de mai jos pentru a analiza CV-ul selectat.",
            ["Check CV quality"] = "Verifică calitatea CV-ului",
            ["Problems found"] = "Probleme găsite",
            ["Recommendations"] = "Recomandări",
            ["Tailor CV"] = "Ajustează CV",
            ["Generate suggestions to adapt your CV for this job."] = "Generează sugestii pentru adaptarea CV-ului la acest job.",
            ["Choose a CV to tailor"] = "Alege un CV de adaptat",
            ["Select one of your created or uploaded CVs. You can also add manual CV text if needed."] = "Selectează un CV creat sau încărcat. Poți adăuga și text manual dacă este nevoie.",
            ["Manual CV text"] = "Text CV manual",
            ["Generate CV suggestions"] = "Generează sugestii pentru CV",
            ["CV needs more information"] = "CV-ul are nevoie de mai multe informații",
            ["Tailored profile summary"] = "Rezumat de profil adaptat",
            ["Important keywords"] = "Cuvinte-cheie importante",
            ["Skills to highlight"] = "Competențe de evidențiat",
            ["Experience to emphasize"] = "Experiență de evidențiat",
            ["CV improvement suggestions"] = "Sugestii de îmbunătățire a CV-ului",
            ["Paste your profile, skills or experience section here..."] = "Lipește aici profilul, competențele sau secțiunea de experiență...",
            ["Interview Preparation"] = "Pregătire interviu",
            ["Interview Prep"] = "Pregătire interviu",
            ["Generate interview questions and preparation tips for this job."] = "Generează întrebări de interviu și sfaturi de pregătire pentru acest job.",
            ["Generate interview prep"] = "Generează pregătirea",
            ["Preparation summary"] = "Rezumat pregătire",
            ["Key skills to review"] = "Competențe importante de revizuit",
            ["Possible interview questions"] = "Posibile întrebări de interviu",
            ["Before the interview"] = "Înainte de interviu",

            // Career test, kept available if you decide to keep it
            ["Career Orientation Test"] = "Test de orientare în carieră",
            ["Important"] = "Important",
            ["Your answers and result are not saved automatically. If you want to keep the result, download the PDF report at the end of the test."] = "Răspunsurile și rezultatul nu sunt salvate automat. Dacă vrei să păstrezi rezultatul, descarcă raportul PDF la finalul testului.",
            ["Question 1 of 60"] = "Întrebarea 1 din 60",
            ["Work style"] = "Stil de lucru",
            ["Question text"] = "Textul întrebării",
            ["Strongly disagree"] = "Dezacord total",
            ["Disagree"] = "Dezacord",
            ["Neutral"] = "Neutru",
            ["Agree"] = "Acord",
            ["Strongly agree"] = "Acord total",
            ["Previous"] = "Înapoi",
            ["Next"] = "Următorul",
            ["Career Test Result"] = "Rezultatul testului de carieră",
            ["Review your result and export it as a PDF report."] = "Revizuiește rezultatul și exportă-l ca raport PDF.",
            ["Temporary result"] = "Rezultat temporar",
            ["This result is not saved automatically. Download the PDF report if you want to keep it after leaving this page."] = "Acest rezultat nu este salvat automat. Descarcă raportul PDF dacă vrei să îl păstrezi după ce părăsești pagina.",
            ["Your profile"] = "Profilul tău",
            ["Career area scores"] = "Scoruri pe arii de carieră",
            ["Recommended career directions"] = "Direcții de carieră recomandate",
            ["Suggested next steps"] = "Pași următori sugerați",
            ["Download PDF report"] = "Descarcă raportul PDF",
            ["Start career test"] = "Începe testul de carieră",
            ["60 questions"] = "60 întrebări",
            ["Discover career directions that may match your interests and the types of work activities you prefer."] =
    "Descoperă direcții de carieră care se pot potrivi intereselor tale și tipurilor de activități pe care le preferi.",
            ["Focus"] = "Se evaluează",
            ["Result"] = "Rezultat",
            ["Interests and preferred work activities"] =
    "Interese și activități de lucru preferate",
            ["Output"] = "Rezultat",
            ["RIASEC profile + PDF report"] =
    "Profil RIASEC + raport PDF",

            // Edit profile/password
            ["Edit Profile"] = "Editează profilul",
            ["Complete optional details used for your career profile."] = "Completează detalii opționale folosite pentru profilul tău de carieră.",
            ["Career preferences"] = "Preferințe de carieră",
            ["Save profile"] = "Salvează profilul",
            ["Your full name"] = "Numele tău complet",
            ["Add phone number"] = "Adaugă număr de telefon",
            ["Add another email"] = "Adaugă alt email",
            ["Add location"] = "Adaugă locație",
            ["Choose career level"] = "Alege nivelul de carieră",
            ["Choose preferred job type"] = "Alege tipul de job preferat",
            ["Change Password"] = "Schimbă parola",
            ["Update your password securely."] = "Actualizează parola în siguranță.",
            ["Password details"] = "Detalii parolă",
            ["Current password"] = "Parola curentă",
            ["New password"] = "Parolă nouă",
            ["Confirm new password"] = "Confirmă parola nouă",
            ["Password must have at least 6 characters."] = "Parola trebuie să aibă cel puțin 6 caractere.",
            ["Save new password"] = "Salvează parola nouă",
            ["Enter current password"] = "Introdu parola curentă",
            ["Enter new password"] = "Introdu parola nouă",

            // Templates / preview sample text
            ["CV Preview"] = "Previzualizare CV",
            ["Preview generated from selected template."] = "Previzualizare generată din șablonul selectat.",
            ["Template preview - Green Professional"] = "Previzualizare șablon - Green Professional",
            ["Template preview - Warm Beige"] = "Previzualizare șablon - Warm Beige",
            ["CONTACT"] = "CONTACT",
            ["SKILLS"] = "COMPETENȚE",
            ["LANGUAGES"] = "LIMBI",
            ["PROFILE"] = "PROFIL",
            ["EXPERIENCE"] = "EXPERIENȚĂ",
            ["EDUCATION"] = "EDUCAȚIE",
            ["PROJECTS"] = "PROIECTE",
            ["BSc Computer Science"] = "Licență Informatică",
            ["CV Builder App"] = "Aplicație pentru creare CV",

            // Code-behind messages that are visible inside labels
            ["Email and password are required."] = "Emailul și parola sunt obligatorii.",
            ["Please enter a valid email address."] = "Introdu o adresă de email validă.",
            ["Invalid email or password."] = "Email sau parolă incorectă.",
            ["All fields are required."] = "Toate câmpurile sunt obligatorii.",
            ["Password and confirmation do not match."] = "Parola și confirmarea nu coincid.",
            ["Account could not be created."] = "Contul nu a putut fi creat.",
            ["Account created successfully. You can now sign in."] = "Contul a fost creat cu succes. Te poți autentifica acum.",
            ["The CV could not be created."] = "CV-ul nu a putut fi creat.",
            ["The CV could not be loaded."] = "CV-ul nu a putut fi încărcat.",
            ["The CV could not be saved."] = "CV-ul nu a putut fi salvat.",
            ["Saved successfully."] = "Salvat cu succes.",
            ["CV saved successfully."] = "CV salvat cu succes.",
            ["Please choose a PDF file."] = "Alege un fișier PDF.",
            ["Please choose a PDF file first."] = "Alege mai întâi un fișier PDF.",
            ["Uploading and extracting information..."] = "Se încarcă și se extrag informațiile...",
            ["The PDF could not be uploaded."] = "PDF-ul nu a putut fi încărcat.",
            ["The CV could not be created from this PDF."] = "CV-ul nu a putut fi creat din acest PDF.",
            ["CV created"] = "CV creat",
            ["The information found in your PDF was copied into a new editable CV. Please review and complete any missing fields."] = "Informațiile găsite în PDF au fost copiate într-un CV nou editabil. Verifică și completează câmpurile lipsă.",
            ["Please choose a file first."] = "Alege mai întâi un fișier.",
            ["Only PDF, DOC and DOCX files are allowed."] = "Sunt permise doar fișiere PDF, DOC și DOCX.",
            ["The file could not be uploaded."] = "Fișierul nu a putut fi încărcat.",
            ["File uploaded successfully."] = "Fișier încărcat cu succes.",
            ["The file could not be opened."] = "Fișierul nu a putut fi deschis.",
            ["The uploaded CV could not be deleted."] = "CV-ul încărcat nu a putut fi șters.",
            ["Are you sure you want to delete this uploaded CV?"] = "Sigur vrei să ștergi acest CV încărcat?",
            ["Are you sure you want to delete this CV?"] = "Sigur vrei să ștergi acest CV?",
            ["The CV could not be deleted."] = "CV-ul nu a putut fi șters.",
            ["Please enter a job keyword before searching."] = "Introdu un cuvânt-cheie pentru job înainte de căutare.",
            ["Searching jobs..."] = "Se caută joburi...",
            ["Loading more jobs..."] = "Se încarcă mai multe joburi...",
            ["Application saved successfully."] = "Aplicarea a fost salvată cu succes.",
            ["The application could not be saved."] = "Aplicarea nu a putut fi salvată.",
            ["The application could not be updated."] = "Aplicarea nu a putut fi actualizată.",
            ["The application could not be deleted."] = "Aplicarea nu a putut fi ștearsă.",
            ["Generating cover letter..."] = "Se generează scrisoarea de intenție...",
            ["The cover letter could not be generated."] = "Scrisoarea de intenție nu a putut fi generată.",
            ["Generating match score..."] = "Se generează scorul de potrivire...",
            ["The match score could not be generated."] = "Scorul de potrivire nu a putut fi generat.",
            ["Generating CV suggestions..."] = "Se generează sugestiile pentru CV...",
            ["CV suggestions could not be generated."] = "Sugestiile pentru CV nu au putut fi generate.",
            ["Generating interview preparation..."] = "Se generează pregătirea pentru interviu...",
            ["Interview preparation could not be generated."] = "Pregătirea pentru interviu nu a putut fi generată.",
            ["Profile saved"] = "Profil salvat",
            ["Your profile details were saved successfully."] = "Detaliile profilului au fost salvate cu succes.",
            ["Profile could not be updated."] = "Profilul nu a putut fi actualizat.",
            ["Password changed"] = "Parolă schimbată",
            ["Your password was changed successfully."] = "Parola a fost schimbată cu succes.",
            ["Password could not be changed."] = "Parola nu a putut fi schimbată.",
            ["Discover career directions that may fit your interests, strengths and preferred work style."] = "Descoperă direcții de carieră care se pot potrivi intereselor tale și tipurilor de activități pe care le preferi.",
            ["Your result is not saved automatically. Download the PDF report at the end if you want to keep it."] = "Rezultatul nu este salvat automat. Descarcă raportul PDF la final dacă vrei să îl păstrezi.",
            ["All applications"] = "Toate aplicările",
            ["You have not saved or tracked any applications yet."] = "Nu ai salvat sau urmărit încă nicio aplicare.",
            ["Delete application"] = "Șterge aplicarea",
            ["Are you sure you want to delete this application?"] = "Sigur vrei să ștergi această aplicare?",
            ["Delete CV"] = "Șterge CV-ul",
            ["Delete uploaded CV"] = "Șterge CV-ul încărcat",
            ["The file could not be opened. Please make sure your phone has an app that can open this file type."] = "Fișierul nu a putut fi deschis. Asigură-te că telefonul are o aplicație care poate deschide acest tip de fișier.",

            // Edit CV and CV section forms - extra labels, placeholders and messages
            ["Profile"] = "Profil",
            ["Professional Summary"] = "Rezumat profesional",
            ["My CVs"] = "CV-urile mele",
            ["Skill name:"] = "Numele competenței:",
            ["Language name:"] = "Numele limbii:",
            ["Language level"] = "Nivel limbă",
            ["Level:"] = "Nivel:",
            ["Example: Beginner, Intermediate, Advanced, Native:"] = "Exemplu: Începător, Intermediar, Avansat, Nativ:",
            ["Edit Education"] = "Editează educația",
            ["Edit Experience"] = "Editează experiența",
            ["Edit Project"] = "Editează proiectul",
            ["Edit Certification"] = "Editează certificarea",
            ["Institution is required."] = "Instituția este obligatorie.",
            ["Degree is required."] = "Diploma / specializarea este obligatorie.",
            ["Job title is required."] = "Titlul jobului este obligatoriu.",
            ["Company is required."] = "Compania este obligatorie.",
            ["Project title is required."] = "Titlul proiectului este obligatoriu.",
            ["Certification name is required."] = "Numele certificării este obligatoriu.",
            ["CV title is required."] = "Titlul CV-ului este obligatoriu.",
            ["End date cannot be before start date."] = "Data de final nu poate fi înaintea datei de început.",
            ["Education could not be saved."] = "Educația nu a putut fi salvată.",
            ["Education could not be updated."] = "Educația nu a putut fi actualizată.",
            ["Experience could not be saved."] = "Experiența nu a putut fi salvată.",
            ["Experience could not be updated."] = "Experiența nu a putut fi actualizată.",
            ["Project could not be saved."] = "Proiectul nu a putut fi salvat.",
            ["Project could not be updated."] = "Proiectul nu a putut fi actualizat.",
            ["Certification could not be saved."] = "Certificarea nu a putut fi salvată.",
            ["Certification could not be updated."] = "Certificarea nu a putut fi actualizată.",
            ["The personal information could not be saved."] = "Informațiile personale nu au putut fi salvate.",
            ["The user could not be loaded."] = "Utilizatorul nu a putut fi încărcat.",
            ["Choose CV photo"] = "Alege fotografia pentru CV",
            ["Only JPG, JPEG, PNG and WEBP images are allowed."] = "Sunt permise doar imagini JPG, JPEG, PNG și WEBP.",
            ["The photo could not be uploaded."] = "Fotografia nu a putut fi încărcată.",
            ["Photo uploaded successfully."] = "Fotografie încărcată cu succes.",
            ["Delete education"] = "Șterge educația",
            ["Delete experience"] = "Șterge experiența",
            ["Delete project"] = "Șterge proiectul",
            ["Delete certification"] = "Șterge certificarea",
            ["Example: University of Oradea"] = "Exemplu: Universitatea din Oradea",
            ["Example: Computer Science"] = "Exemplu: Informatică",
            ["Example: Software development, databases, web technologies..."] = "Exemplu: dezvoltare software, baze de date, tehnologii web...",
            ["Example: Junior Software Developer"] = "Exemplu: Junior Software Developer",
            ["Example: Microsoft"] = "Exemplu: Microsoft",
            ["Describe your responsibilities, achievements or technologies used..."] = "Descrie responsabilitățile, realizările sau tehnologiile folosite...",
            ["Example: Career Guide Mobile App"] = "Exemplu: Aplicația mobilă Career Guide",
            ["Example: C#, .NET MAUI, ASP.NET Core, SQLite"] = "Exemplu: C#, .NET MAUI, ASP.NET Core, SQLite",
            ["Describe what the project does and your contribution..."] = "Descrie ce face proiectul și contribuția ta...",

            ["Choose your PDF CV"] = "Alege CV-ul PDF",
            ["Please choose a PDF file."] = "Te rog alege un fișier PDF.",
            ["Please choose a PDF file first."] = "Te rog alege mai întâi un fișier PDF.",
            ["Imported CV"] = "CV importat",
            ["Uploading and extracting information..."] = "Se încarcă și se extrag informațiile...",
            ["The PDF could not be uploaded."] = "PDF-ul nu a putut fi încărcat.",
            ["The CV could not be created from this PDF."] = "CV-ul nu a putut fi creat din acest PDF.",
            ["CV created"] = "CV creat",
            ["The information found in your PDF was copied into a new editable CV. Please review and complete any missing fields."] =
    "Informațiile găsite în PDF au fost copiate într-un CV nou editabil. Verifică și completează câmpurile lipsă.",
            ["An error occurred while choosing the PDF file."] = "A apărut o eroare la alegerea fișierului PDF.",
            ["An error occurred while importing the CV."] = "A apărut o eroare la importarea CV-ului.",
            ["OK"] = "OK",

            ["Choose a CV to copy"] = "Alege un CV de copiat",
            ["The selected CV could not be found."] = "CV-ul selectat nu a putut fi găsit.",
            ["Copy CV"] = "Copiază CV-ul",
            ["Enter a title for the new CV:"] = "Introdu un titlu pentru noul CV:",
            ["This is your base CV."] = "Acesta este CV-ul tău de bază.",
            ["The PDF could not be opened."] = "PDF-ul nu a putut fi deschis.",
            ["No description available for this job."] = "Nu există descriere disponibilă pentru acest job.",
            ["The preview could not be loaded inside the app. You can still use Export to open the PDF."] = "Previzualizarea nu a putut fi încărcată în aplicație. Poți folosi în continuare Export pentru a deschide PDF-ul.",
            ["Already saved"] = "Deja salvat",
            ["This job is already saved or tracked in My Applications."] = "Acest job este deja salvat sau urmărit în Aplicările mele.",
            ["This job was saved successfully. You can find it in My Applications with status Saved."] = "Acest job a fost salvat cu succes. Îl găsești în Aplicările mele cu statusul Salvat.",
            ["Success"] = "Succes",
            ["Application saved"] = "Aplicare salvată",
            ["This job application was saved successfully."] = "Această aplicare a fost salvată cu succes.",
            ["This job is already saved or tracked."] = "Acest job este deja salvat sau urmărit.",
            ["The job could not be saved."] = "Jobul nu a putut fi salvat.",
            ["Job saved"] = "Job salvat",
            ["Job saved successfully."] = "Job salvat cu succes.",
            ["The job was saved successfully."] = "Jobul a fost salvat cu succes.",
            ["Missing link"] = "Link lipsă",
            ["This job does not have an application link."] = "Acest job nu are link de aplicare.",
            ["This job does not have a link to the full post."] = "Acest job nu are link către anunțul complet.",
            ["The job link could not be opened."] = "Linkul jobului nu a putut fi deschis.",
            ["The full job post could not be opened."] = "Anunțul complet nu a putut fi deschis.",
            ["The file could not be deleted."] = "Fișierul nu a putut fi șters.",
            ["PDF preview loaded."] = "Previzualizarea PDF a fost încărcată.",
            ["PDF preview is not available for this file type."] = "Previzualizarea PDF nu este disponibilă pentru acest tip de fișier.",
            ["The selected CV could not be loaded."] = "CV-ul selectat nu a putut fi încărcat.",
            ["Choose a CV or paste CV text first."] = "Alege un CV sau lipește mai întâi textul CV-ului.",
            ["CV suggestions generated successfully."] = "Sugestiile pentru CV au fost generate cu succes.",
            ["All password fields are required."] = "Toate câmpurile pentru parolă sunt obligatorii.",
            ["New password must have at least 6 characters."] = "Parola nouă trebuie să aibă cel puțin 6 caractere.",
            ["New password and confirmation do not match."] = "Parola nouă și confirmarea nu coincid.",
            ["New password must be different from the current password."] = "Parola nouă trebuie să fie diferită de parola curentă.",
            ["Full name is required."] = "Numele complet este obligatoriu.",
            ["Generating recommendations..."] = "Se generează recomandările...",
            ["AI recommendations generated."] = "Recomandările AI au fost generate.",
            ["Could not generate AI recommendations."] = "Recomandările AI nu au putut fi generate.",
            ["Downloading PDF report..."] = "Se descarcă raportul PDF...",
            ["PDF report downloaded."] = "Raportul PDF a fost descărcat.",
            ["PDF report could not be downloaded."] = "Raportul PDF nu a putut fi descărcat.",
            ["Leave result?"] = "Părăsești rezultatul?",
            ["Your result is not saved automatically. Leave this page without downloading the PDF report?"] = "Rezultatul nu este salvat automat. Părăsești pagina fără să descarci raportul PDF?",
            ["Stay"] = "Rămâi",
            ["Leave"] = "Părăsește",
            ["Use local fallback"] = "Folosește rezultat local",
            ["Gemini did not return a valid response. Local recommendations are shown instead."] = "Gemini nu a returnat un răspuns valid. Sunt afișate recomandări locale.",
            ["Example: Microsoft Azure Fundamentals"] = "Exemplu: Microsoft Azure Fundamentals",
        };

        private static readonly Dictionary<string, string> RomanianToEnglish = EnglishToRomanian
            .GroupBy(pair => pair.Value, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Key, StringComparer.Ordinal);

        public static string TranslateText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value ?? string.Empty;

            if (DoNotTranslate.Contains(value.Trim()))
                return "Career Guide";

            if (LanguageService.IsRomanian)
            {
                if (EnglishToRomanian.TryGetValue(value, out var romanian))
                    return romanian;

                var dynamicRomanian = TranslateDynamicEnglishToRomanian(value);
                return dynamicRomanian ?? value;
            }

            if (RomanianToEnglish.TryGetValue(value, out var english))
                return english;

            var dynamicEnglish = TranslateDynamicRomanianToEnglish(value);
            return dynamicEnglish ?? value;
        }

        private static string? TranslateDynamicEnglishToRomanian(string value)
        {
            var welcomeMatch = Regex.Match(value, @"^Welcome, (.+)\. Manage your CVs, jobs and applications in one place\.$");
            if (welcomeMatch.Success)
                return $"Bun venit, {welcomeMatch.Groups[1].Value}. Gestionează CV-urile, joburile și aplicările într-un singur loc.";

            var allApplicationsMatch = Regex.Match(value, @"^Showing all applications \((\d+)\)\.$");
            if (allApplicationsMatch.Success)
                return $"Se afișează toate aplicările ({allApplicationsMatch.Groups[1].Value}).";

            var filteredApplicationsMatch = Regex.Match(value, @"^Showing (.+) applications \((\d+)\)\.$");
            if (filteredApplicationsMatch.Success)
            {
                var status = TranslateText(ToTitleCase(filteredApplicationsMatch.Groups[1].Value));
                return $"Se afișează aplicările cu statusul {status.ToLower()} ({filteredApplicationsMatch.Groups[2].Value}).";
            }

            var missingStatusMatch = Regex.Match(value, @"^You do not have applications with the status (.+) yet\.$");
            if (missingStatusMatch.Success)
            {
                var status = TranslateText(missingStatusMatch.Groups[1].Value);
                return $"Nu ai încă aplicări cu statusul {status.ToLower()}.";
            }

            var interviewInsightMatch = Regex.Match(value, @"^You have (\d+) application\(s\) in the interview stage\. Use Interview Prep to prepare before the next meeting\.$");
            if (interviewInsightMatch.Success)
                return $"Ai {interviewInsightMatch.Groups[1].Value} aplicare/aplicări în etapa de interviu. Folosește pregătirea pentru interviu înainte de următoarea întâlnire.";

            var offerInsightMatch = Regex.Match(value, @"^Great progress\. You have (\d+) offer\(s\)\. Review your applications and keep tracking the next steps\.$");
            if (offerInsightMatch.Success)
                return $"Progres bun. Ai {offerInsightMatch.Groups[1].Value} ofertă/oferte. Revizuiește aplicările și urmărește pașii următori.";

            var periodMatch = Regex.Match(value, @"^Showing activity for (last 7 days|last 30 days|last 90 days|all time)\.$", RegexOptions.IgnoreCase);
            if (periodMatch.Success)
            {
                return periodMatch.Groups[1].Value.ToLowerInvariant() switch
                {
                    "last 7 days" => "Se afișează activitatea pentru ultimele 7 zile.",
                    "last 30 days" => "Se afișează activitatea pentru ultimele 30 de zile.",
                    "last 90 days" => "Se afișează activitatea pentru ultimele 90 de zile.",
                    "all time" => "Se afișează activitatea pentru toată perioada.",
                    _ => null
                };
            }

            var qualityForMatch = Regex.Match(value, @"^Quality analysis for: (.+)$");
            if (qualityForMatch.Success)
                return $"Analiză calitate pentru: {qualityForMatch.Groups[1].Value}";

            var usableCvMatch = Regex.Match(value, @"^This CV is usable, but it still has (\d+) area\(s\) that should be improved before applying\.$");
            if (usableCvMatch.Success)
                return $"Acest CV este utilizabil, dar mai are {usableCvMatch.Groups[1].Value} zonă/zone care ar trebui îmbunătățite înainte de aplicare.";

            var deleteItemMatch = Regex.Match(value, @"^Delete (.+)\?$", RegexOptions.IgnoreCase);
            if (deleteItemMatch.Success)
                return $"Ștergi {deleteItemMatch.Groups[1].Value}?";

            var cvListErrorMatch = Regex.Match(value, @"^CV list could not be loaded: (.+)$");
            if (cvListErrorMatch.Success)
                return $"Lista de CV-uri nu a putut fi încărcată: {cvListErrorMatch.Groups[1].Value}";



            var searchCompletedMatch = Regex.Match(value, @"^Search completed\. Found (\d+) visible jobs\.$");
            if (searchCompletedMatch.Success)
                return $"Căutare finalizată. Au fost găsite {searchCompletedMatch.Groups[1].Value} joburi vizibile.";

            var loadedMoreJobsMatch = Regex.Match(value, @"^Loaded more jobs\. Showing (\d+) jobs after filters\.$");
            if (loadedMoreJobsMatch.Success)
                return $"Au fost încărcate mai multe joburi. Se afișează {loadedMoreJobsMatch.Groups[1].Value} joburi după filtre.";

            var loadedJobsSummaryMatch = Regex.Match(value, @"^Showing (\d+) of (\d+) loaded jobs\.$");
            if (loadedJobsSummaryMatch.Success)
                return $"Se afișează {loadedJobsSummaryMatch.Groups[1].Value} din {loadedJobsSummaryMatch.Groups[2].Value} joburi încărcate.";

            var templatePreviewMatch = Regex.Match(value, @"^Template preview - (.+)$");
            if (templatePreviewMatch.Success)
                return $"Previzualizare șablon - {templatePreviewMatch.Groups[1].Value}";

            var deleteNamedItemMatch = Regex.Match(value, @"^Delete (.+)\?$", RegexOptions.IgnoreCase);
            if (deleteNamedItemMatch.Success)
                return $"Ștergi {deleteNamedItemMatch.Groups[1].Value}?";

            return null;
        }

        private static string? TranslateDynamicRomanianToEnglish(string value)
        {
            var welcomeMatch = Regex.Match(value, @"^Bun venit, (.+)\. Gestionează CV-urile, joburile și aplicările într-un singur loc\.$");
            if (welcomeMatch.Success)
                return $"Welcome, {welcomeMatch.Groups[1].Value}. Manage your CVs, jobs and applications in one place.";

            var allApplicationsMatch = Regex.Match(value, @"^Se afișează toate aplicările \((\d+)\)\.$");
            if (allApplicationsMatch.Success)
                return $"Showing all applications ({allApplicationsMatch.Groups[1].Value}).";

            var periodMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Se afișează activitatea pentru ultimele 7 zile."] = "Showing activity for last 7 days.",
                ["Se afișează activitatea pentru ultimele 30 de zile."] = "Showing activity for last 30 days.",
                ["Se afișează activitatea pentru ultimele 90 de zile."] = "Showing activity for last 90 days.",
                ["Se afișează activitatea pentru toată perioada."] = "Showing activity for all time."
            };

            var searchCompletedMatch = Regex.Match(value, @"^Căutare finalizată\. Au fost găsite (\d+) joburi vizibile\.$");
            if (searchCompletedMatch.Success)
                return $"Search completed. Found {searchCompletedMatch.Groups[1].Value} visible jobs.";

            var loadedMoreJobsMatch = Regex.Match(value, @"^Au fost încărcate mai multe joburi\. Se afișează (\d+) joburi după filtre\.$");
            if (loadedMoreJobsMatch.Success)
                return $"Loaded more jobs. Showing {loadedMoreJobsMatch.Groups[1].Value} jobs after filters.";

            var loadedJobsSummaryMatch = Regex.Match(value, @"^Se afișează (\d+) din (\d+) joburi încărcate\.$");
            if (loadedJobsSummaryMatch.Success)
                return $"Showing {loadedJobsSummaryMatch.Groups[1].Value} of {loadedJobsSummaryMatch.Groups[2].Value} loaded jobs.";

            return periodMap.TryGetValue(value, out var english) ? english : null;
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            value = value.Trim();
            return char.ToUpperInvariant(value[0]) + value[1..];
        }


        public static Task DisplayAlertAsync(Page page, string title, string message, string cancel = "OK")
        {
            return page.DisplayAlert(
                TranslateText(title),
                TranslateText(message),
                TranslateText(cancel));
        }

        public static Task<bool> DisplayConfirmAsync(Page page, string title, string message, string accept = "Yes", string cancel = "No")
        {
            return page.DisplayAlert(
                TranslateText(title),
                TranslateText(message),
                TranslateText(accept),
                TranslateText(cancel));
        }

        public static void ApplyToPage(Page? page)
        {
            if (page == null)
                return;

            ApplyToElement(page);
        }

        public static void ApplyToElement(Element? element)
        {
            if (element == null)
                return;

            WatchElement(element);
            TranslateElement(element);

            foreach (var child in GetChildren(element))
                ApplyToElement(child);
        }

        private static void WatchElement(Element element)
        {
            if (element is not BindableObject bindable)
                return;

            if ((bool)bindable.GetValue(IsWatchingProperty))
                return;

            bindable.SetValue(IsWatchingProperty, true);
            bindable.PropertyChanged += OnElementPropertyChanged;
        }

        private static void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Element element)
                return;

            if (sender is BindableObject bindable && (bool)bindable.GetValue(IsUpdatingProperty))
                return;

            var propertyName = e.PropertyName ?? string.Empty;

            if (propertyName is nameof(Label.Text)
                or nameof(Button.Text)
                or nameof(Entry.Placeholder)
                or nameof(Editor.Placeholder)
                or nameof(SearchBar.Placeholder)
                or nameof(Page.Title)
                or nameof(Picker.Title)
                or nameof(RadioButton.Content)
                or nameof(CheckBox.IsChecked))
            {
                TranslateElement(element);
            }
        }

        private static void TranslateElement(Element element)
        {
            var bindableTarget = element as BindableObject;

            if (bindableTarget != null && (bool)bindableTarget.GetValue(IsUpdatingProperty))
                return;

            bindableTarget?.SetValue(IsUpdatingProperty, true);

            try
            {
                switch (element)
                {
                    case Page page:
                        page.Title = TranslateText(page.Title);
                        break;

                    case Label label:
                        label.Text = TranslateText(label.Text);
                        break;

                    case Button button:
                        button.Text = TranslateText(button.Text);
                        break;

                    case Entry entry:
                        entry.Placeholder = TranslateText(entry.Placeholder);
                        break;

                    case Editor editor:
                        editor.Placeholder = TranslateText(editor.Placeholder);
                        break;

                    case SearchBar searchBar:
                        searchBar.Placeholder = TranslateText(searchBar.Placeholder);
                        break;

                    case Picker picker:
                        TranslatePicker(picker);
                        break;

                    case RadioButton radioButton when radioButton.Content is string text:
                        radioButton.Content = TranslateText(text);
                        break;
                }
            }
            finally
            {
                bindableTarget?.SetValue(IsUpdatingProperty, false);
            }
        }

        private static void TranslatePicker(Picker picker)
        {
            picker.Title = TranslateText(picker.Title);

            if (picker.ItemsSource != null || picker.Items.Count == 0)
                return;

            var selectedIndex = picker.SelectedIndex;
            var translatedItems = picker.Items.Select(TranslateText).ToList();

            picker.Items.Clear();

            foreach (var item in translatedItems)
                picker.Items.Add(item);

            if (selectedIndex >= 0 && selectedIndex < picker.Items.Count)
                picker.SelectedIndex = selectedIndex;
        }

        private static IEnumerable<Element> GetChildren(Element element)
        {
            var children = new List<Element>();

            void Add(Element? child)
            {
                if (child == null || ReferenceEquals(child, element) || children.Contains(child))
                    return;

                children.Add(child);
            }

            switch (element)
            {
                case ContentPage page when page.Content != null:
                    Add(page.Content);
                    break;

                case ScrollView scrollView when scrollView.Content != null:
                    Add(scrollView.Content);
                    break;

                case ContentView contentView when contentView.Content != null:
                    Add(contentView.Content);
                    break;

                case Border border when border.Content != null:
                    Add(border.Content);
                    break;

                case Layout layout:
                    foreach (var child in layout.Children.OfType<Element>())
                        Add(child);
                    break;

                case TabbedPage tabbedPage:
                    foreach (var child in tabbedPage.Children)
                        Add(child);
                    break;

                case FlyoutPage flyoutPage:
                    Add(flyoutPage.Flyout);
                    Add(flyoutPage.Detail);
                    break;

                case NavigationPage navigationPage:
                    foreach (var page in navigationPage.Navigation.NavigationStack)
                        Add(page);
                    break;

                case CollectionView collectionView:
                    if (collectionView.EmptyView is Element emptyElement)
                        Add(emptyElement);
                    break;
            }

            if (element is IVisualTreeElement visualTreeElement)
            {
                foreach (var visualChild in visualTreeElement.GetVisualChildren().OfType<Element>())
                    Add(visualChild);
            }

            return children;
        }
    }
}
