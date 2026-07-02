# Career Guide

Acest repository conține codul sursă al aplicației Career Guide, dezvoltată în cadrul lucrării de licență.

Aplicația este destinată persoanelor aflate în procesul de căutare a unui loc de muncă și oferă funcționalități pentru crearea și administrarea CV-urilor, exportarea CV-ului în format PDF, căutarea joburilor, urmărirea aplicărilor și utilizarea unor funcții asistive. Unele dintre aceste funcții folosesc servicii externe de inteligență artificială pentru generarea de sugestii, scrisori de intenție sau întrebări pentru interviu.

## Repository

Adresa repository-ului este:
https://github.com/bianca-nistor/Licenta

Versiunea finală a proiectului se află pe branch-ul:
final-clean


## Livrabilele proiectului

Repository-ul conține următoarele livrabile:
JobCv.Mobile   - aplicația mobilă dezvoltată în .NET MAUI
Jobcv.Api      - backend-ul aplicației, dezvoltat în ASP.NET Core Web API
README.md      - fișierul cu descrierea proiectului și pașii de compilare/rulare

## Tehnologii utilizate

Aplicația utilizează următoarele tehnologii:

- C#;
- .NET MAUI;
- ASP.NET Core Web API;
- Entity Framework Core;
- Azure App Service;
- Azure SQL Database;
- Adzuna API;
- Gemini API;
- Visual Studio 2022.

## Descriere generală

Aplicația este formată din două componente principale:

1. Aplicația mobilă - proiectul `JobCv.Mobile`, realizat în .NET MAUI. Aceasta oferă interfața cu utilizatorul și trimite cereri HTTP către backend.
2. Backend-ul - proiectul `Jobcv.Api`, realizat în ASP.NET Core Web API. Acesta gestionează autentificarea, datele utilizatorilor, CV-urile, aplicările la joburi, generarea PDF-urilor și integrarea cu serviciile externe.

## Funcționalități principale

Aplicația include următoarele funcționalități:

- înregistrare și autentificare utilizator;
- administrarea profilului utilizatorului;
- creare, editare, duplicare și ștergere CV;
- completarea secțiunilor unui CV: competențe, educație, experiență, proiecte, limbi străine și certificări;
- alegerea unui template pentru CV;
- previzualizarea CV-ului;
- exportarea CV-ului în format PDF;
- încărcarea unei fotografii pentru CV;
- importarea informațiilor dintr-un CV PDF într-un CV editabil;
- căutarea joburilor printr-un serviciu extern;
- salvarea aplicărilor la joburi;
- urmărirea statusului aplicărilor;
- verificarea calității CV-ului prin reguli interne de validare;
- estimarea compatibilității dintre CV și job;
- generarea de sugestii pentru adaptarea CV-ului la un anumit job;
- generarea unei scrisori de intenție pe baza CV-ului și a descrierii jobului;
- generarea de întrebări orientative pentru pregătirea interviului;
- test de orientare în carieră și recomandări pe baza răspunsurilor utilizatorului;
- suport pentru traducere/localizare în aplicația mobilă.

## Cerințe pentru compilare

Pentru compilarea proiectului sunt necesare:

- Visual Studio 2022;
- .NET SDK 9.0;
- workload-ul pentru .NET MAUI instalat în Visual Studio;
- conexiune la internet;
- acces la baza de date configurată pentru backend;
- configurarea cheilor necesare pentru serviciile externe, dacă se rulează backend-ul local complet.

## Configurarea backend-ului

Backend-ul se află în folderul:
Jobcv.Api

Pentru rularea completă a backend-ului sunt necesare setările pentru:

- connection string-ul bazei de date;
- Adzuna API;
- Gemini API;
- alte setări locale necesare aplicației.

Aceste setări pot fi configurate în fișierele:

appsettings.json
appsettings.Development.json

sau în setările din Azure App Service, în cazul backend-ului publicat.

Din motive de securitate, cheile API și connection string-urile reale nu sunt publicate într-un repository public.

## Compilarea și rularea backend-ului

Backend-ul se află în folderul `Jobcv.Api` și poate fi compilat și rulat din Visual Studio 2022.

Pașii de rulare sunt:

1. Se deschide soluția proiectului în Visual Studio 2022.
2. Se selectează proiectul `Jobcv.Api` ca proiect de pornire.
3. Se verifică fișierele de configurare sau setările locale necesare pentru baza de date și serviciile externe.
4. Se apasă butonul Start din Visual Studio.

În versiunea finală, aplicația mobilă este configurată să comunice cu backend-ul publicat în Azure prin adresa:
https://career-guide-d7dubecfb3hchcf7.swedencentral-01.azurewebsites.net/swagger/index.html

## Compilarea aplicației mobile

Aplicația mobilă se află în folderul:
JobCv.Mobile

Pentru compilare:

1. Se deschide proiectul sau soluția în Visual Studio 2022.
2. Se selectează proiectul `JobCv.Mobile`.
3. Se alege platforma dorită, de exemplu:
   - Windows Machine;
   - Android Emulator;
   - dispozitiv Android conectat.
4. Se rulează comanda:
Build → Rebuild Solution

## Lansarea aplicației

Pentru lansarea aplicației mobile din Visual Studio:

1. Se deschide proiectul `JobCv.Mobile`.
2. Se selectează platforma de rulare dorită.
3. Se apasă butonul Start.
În cazul rulării pe Android Emulator sau pe un telefon conectat, Visual Studio instalează automat aplicația pe dispozitivul selectat și o pornește.

## Baza de date

Aplicația utilizează o bază de date Azure SQL Database. Accesul la baza de date este realizat prin backend, folosind Entity Framework Core.
Aplicația mobilă nu accesează direct baza de date. Fluxul de comunicare este:
JobCv.Mobile → Jobcv.Api → Azure SQL Database
