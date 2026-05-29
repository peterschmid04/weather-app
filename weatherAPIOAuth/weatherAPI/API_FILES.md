# Backend-API-Dateien

Diese Datei erklaert das ASP.NET Core Backend-Projekt. Alle Pfade sind relativ
zu `weatherAPIOAuth/weatherAPI`.

## Program.cs

Zentrale Startdatei der Minimal API. Aufgaben:

- Umgebungsvariablen lesen und Pflichtwerte validieren.
- Logging einrichten.
- EF Core DbContext fuer PostgreSQL registrieren.
- Auth0 JWT Bearer Authentication konfigurieren.
- CORS fuer lokale Frontend-Origins setzen.
- Rate Limiting registrieren.
- Swagger fuer lokale API-Pruefung konfigurieren.
- Datenbankmigrationen beim Start anwenden.
- REST-Endpunkte mappen.

Endpunktgruppen in `Program.cs`:

- Wetter: `/weather`, `/forecast`, `/uv`, `/airquality`
- Profil/Zugriff: `/my-profile`, `/access`
- Verlauf: `/history`
- Favoriten: `/favorites`
- Theme: `/theme`
- Wetterstationen: `/stations`
- Freigaben: `/station-shares`

## Data

`Data/WeatherDbContext.cs`

EF-Core-DbContext. Definiert Tabellen, Beziehungen, Indizes und Delete-Verhalten.
Diese Datei ist die zentrale ORM-Abbildung fuer PostgreSQL.

`Data/DatabaseMigrationExtensions.cs`

Starthelfer fuer automatische Migrationen. Er nutzt Retry-Logik, damit das
Backend einen frisch gestarteten PostgreSQL-Container abwarten kann.

`Data/WeatherDbContextFactory.cs`

Design-Time-Factory fuer EF-Core-CLI-Befehle wie `dotnet ef migrations add`.
Runtime nutzt die gleiche Connection-String-Variable wie Docker Compose.

## Models/Database

Diese Klassen sind Datenbanktabellen:

- `UserProfile.cs`: Lokales Profil pro Auth0-User-ID.
- `City.cs`: Normalisierte Stadt-/Land-Kombination.
- `SearchHistory.cs`: Suchverlauf eines Nutzers.
- `FavoriteCity.cs`: Favoriten eines Nutzers, inklusive Standardfavorit.
- `UserThemePreference.cs`: Gespeichertes GUI-Theme.
- `WeatherStation.cs`: Eigene Wetterstationen.
- `WeatherStationMeasurement.cs`: Messwerte einer Station.
- `WeatherStationShare.cs`: Freigaben fuer andere Auth0-Nutzer.
- `WeatherRequestLog.cs`: Technisches Request-Log fuer Wetterabfragen.

Die Nutzerverwaltung selbst liegt nicht in der Datenbank. Auth0 verwaltet Login,
Registrierung, Passwort-Hashing und Social Login.

## Models/Dto

DTOs sind Request- und Response-Modelle fuer die API:

- `Weather.cs`: Aktuelle Wetterdaten fuer das Frontend.
- `Forecast.cs`: Forecast-Karte fuer einen Tag.
- `Uv.cs`: UV-Antwort.
- `AirQuality.cs`: Luftqualitaetsantwort.
- `UserDataDtos.cs`: Profil, Verlauf, Favoriten und Theme.
- `WeatherStationDtos.cs`: Stationen, Messwerte und Freigaben.
- `AccessOverviewResponse.cs`: Auth0-Rollen und Permissions fuer die UI.

DTOs trennen API-Vertraege von Datenbanktabellen. So kann sich das interne
Datenmodell aendern, ohne direkt das Frontend zu brechen.

## Models/External

Diese Klassen bilden rohe OpenWeatherMap-JSON-Antworten ab:

- `OpenWeatherResponse.cs`: Aktuelles Wetter.
- `ForecastApiResponse.cs`: Forecast-Root.
- `ForecastDay.cs`: Einzelner Forecast-Zeitpunkt.
- `ForecastCity.cs`: Forecast-Metadaten.
- `ForecastMain.cs`: Forecast-Temperaturen.
- `Coord.cs`: Koordinaten.
- `Main.cs`: Temperatur und Luftfeuchtigkeit.
- `Sys.cs`: Land, Sonnenaufgang und Sonnenuntergang.
- `WeatherInfo.cs`: Beschreibung, Icon und Wettercode.
- `Wind.cs`: Winddaten.

Diese Modelle werden nur fuer die externe API genutzt. Das Frontend sieht die
bereinigten DTOs aus `Models/Dto`.

## Services

`Services/OpenWeatherApiService.cs`

Kapselt HTTP-Aufrufe an OpenWeatherMap. Baut URLs, liest den API-Key aus
Konfiguration und deserialisiert Antworten.

`Services/WeatherService.cs`

Wandelt aktuelle OpenWeatherMap-Daten in das interne `Weather` DTO um.

`Services/ForecastService.cs`

Gruppiert Forecast-Daten in Tageskarten fuer das Frontend.

`Services/UvService.cs`

Kleine Service-Schicht fuer UV-Daten.

`Services/AirQualityService.cs`

Kleine Service-Schicht fuer Luftqualitaetsdaten.

`Services/Interfaces`

Interfaces fuer Services. Sie erleichtern Dependency Injection und Unit-Tests.

## Security

`Security/RegionAuthorization.cs`

Wertet Auth0-Permissions aus. Nutzer ohne besondere Permission haben
Standardzugriff auf Deutschland. Weitere Regionen werden ueber Auth0-Permissions
freigegeben.

## Logging

`Logging/DetailedFileLoggerProvider.cs`

Optionaler Datei-Logger fuer ausfuehrliche lokale Logs. Der Logordner wird erst
erstellt, wenn Logging per `.env` aktiviert ist.

## Migrations

EF-Core-Migrationsdateien. Sie beschreiben die Entwicklung des Datenbankschemas.
Diese Dateien werden normalerweise nicht manuell bearbeitet.

## appsettings

`appsettings.json`

Basiskonfiguration ohne echte Secrets.

`appsettings.Development.json`

Entwicklungs-Logging. Docker Compose ueberschreibt produktnahe Werte ueber
Umgebungsvariablen.

## weatherAPI.http

Kleine Scratch-Datei fuer IDEs, mit der REST-Anfragen manuell getestet werden
koennen. Authentifizierte Endpunkte brauchen ein Auth0 Bearer Token.

