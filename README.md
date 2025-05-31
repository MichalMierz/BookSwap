# BookSwap – platforma wymiany książek między użytkownikami
Repozytorium: https://github.com/Eryk333/BookSwap

BookSwap to webowa aplikacja umożliwiająca użytkownikom wystawianie własnych książek oraz bezpieczną wymianę egzemplarzy z innymi. Projekt powstał jako realizacja zadania zaliczeniowego z przedmiotu Programowanie zaawansowane.




# Kluczowe funkcjonalności

### Rejestracja i role użytkowników
Logowanie przez Microsoft Identity; dwie role: Użytkownik i Moderator
Powiązany model: User

### Katalog książek (CRUD)
Dodawanie, edycja i usuwanie własnych egzemplarzy (tytuł, autor, gatunek, stan, zdjęcie okładki)
Powiązany model: Book

### Wyszukiwanie i filtrowanie
Publiczna lista książek z filtrami (tytuł, autor, gatunek, stan, dostępność)
Powiązany model: Book, Genre

### Mechanizm wymiany
Użytkownik składa propozycję wymiany książki innej osobie; druga strona może ją przyjąć lub odrzucić. Status książek aktualizuje się automatycznie
Powiązany model: Exchange

### Historia wymian i profil (opcjonalne)
Widok „Moje wymiany” z listą oczekujących i zakończonych transakcji
Powiązany model: Exchange

### Panel moderatora
Lista zgłoszonych ogłoszeń z możliwością ukrycia książki lub zablokowania konta użytkownika
Powiązany model: Report




# Stack technologiczny

ASP.NET Core 8 MVC
Entity Framework Core 8 (Code‑First)
Microsoft Identity do zarządzania kontami i rolami
Razor Pages + Bootstrap 5 (UI)
SQLite (środowisko deweloperskie) / SQL Server (produkcja)




# Wymagania wstępne

.NET SDK	≥ 8.0
Visual Studio 2022	≥ 17.10 (lub VS Code z C# Dev Kit)
SQL Server Express lub SQLite	dowolna zgodna




# Instalacja i uruchomienie

### 1. Klonuj repozytorium
$ git clone https://github.com/Eryk333/BookSwap.git
$ cd BookSwap

### 2. Przygotuj bazę danych
$ dotnet tool install --global dotnet-ef   # jeśli nie masz
$ dotnet ef database update                # tworzy bazę i migracje

### 3. Uruchom aplikację
$ dotnet run                                # aplikacja wystartuje na http://localhost:5000

Uwaga: Domyślnie wykorzystywana jest baza SQLite zapisana w pliku books.db. Zmień ciąg połączenia w appsettings.json, jeśli chcesz użyć SQL Server lub innego silnika.




# Struktura projektu (najważniejsze katalogi)

BookSwap/
├── BookSwap.Domain/        # encje i logika domenowa
├── BookSwap.Infrastructure # konfiguracja EF Core, migracje, repozytoria
├── BookSwap.Web/           # warstwa MVC (Controllers, Views, ViewModels)
└── BookSwap.Tests/         # testy jednostkowe (xUnit)




# Schemat danych (skrót)

User (Id) 1‑N Book
User (Id) 1‑N Exchange (UserA / UserB)
Book (Id) N‑1 Genre
Book (Id) 1‑N Exchange (BookOffered / BookRequested)
Report (Id) N‑1 Book




# Licencja

Projekt udostępniany jest na licencji MIT. Szczegóły w pliku LICENSE.




# Autorzy

Eryk Łopuszewski
