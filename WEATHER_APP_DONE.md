# Weather App erledigt

Stand: 29.05.2026

Diese Datei sammelt Punkte, die nach aktuellem Projektstand bereits erledigt sind oder von den Review-Listen als positiv bewertet wurden. Sie ersetzt die abgehakten Teile aus den alten Todo-Dateien.

## Projektbasis

- [x] Projekttitel festgelegt: containerisierte Wetterdaten-Plattform mit Backend-Service, Datenpersistenz und webbasierter Benutzeroberflaeche.
- [x] Projektbeschreibung formuliert: React-Frontend, ASP.NET-Core-Minimal-API-Backend und relationale PostgreSQL-Datenbank.
- [x] Fokus als nachvollziehbarer Prototyp fuer die Pruefungsleistung festgelegt.
- [x] Frontend: React mit JavaScript.
- [x] Backend: C# mit ASP.NET Core Minimal API.
- [x] Authentifizierung: Auth0 mit JWT / Bearer Token.
- [x] Datenbank: PostgreSQL statt Microsoft SQL Server.
- [x] ORM: Entity Framework Core als C#-Variante zu SQLAlchemy.
- [x] Externe API: OpenWeatherMap API.
- [x] Infrastruktur: Docker und Docker Compose.
- [x] Versionskontrolle: Git und GitHub.
- [x] Entwicklungsumgebung: VS Code / JetBrains Rider geeignet.

## Projektstand aufgeraeumt

- [x] Geprueft, welche Ordner wirklich gebraucht werden.
- [x] Alten/generierten Ordner `weatherAPI/.../obj` aus dem Repo-Konzept entfernt.
- [x] Alte `weatherAPI`-Ablage mit Build-/IDE-Resten entfernt.
- [x] `.DS_Store` als macOS-Systemmuell entfernt und ignoriert.
- [x] OneDrive-Problemdatei ` WeatherApiServicesTests.cs` in `WeatherApiServicesTests.cs` umbenannt.
- [x] `.gitignore` fuer `bin`, `obj`, `node_modules`, `.env`, Build-Artefakte und IDE-Dateien ergaenzt.
- [x] Lokale `.env` aus Git entfernt.
- [x] Alte Frontend-`.env` entfernt, damit lokale Werte an einem Ort liegen.
- [x] `.env.example` fuer alle Services vorbereitet.
- [x] Docker-Compose-Basis mit Volumes fuer lokale Build-Artefakte aufgenommen.
- [x] Lokale Build-Artefakte werden in Docker Volumes statt Projektordner geschrieben.

## Docker Compose und Deployment

- [x] Bestehende Services behalten: `frontend`, `backend`.
- [x] Service `db` hinzugefuegt.
- [x] pgAdmin als weiterer Service hinzugefuegt.
- [x] pgAdmin ueber `http://localhost:5050` erreichbar gemacht.
- [x] pgAdmin mit vorbereiteter Server-Verbindung zu PostgreSQL ausgestattet.
- [x] PostgreSQL statt Microsoft SQL Server verwendet.
- [x] PostgreSQL Image vorbereitet.
- [x] Datenbank-Dateien in Docker Volume gelegt.
- [x] Backend startet erst, wenn PostgreSQL erreichbar ist.
- [x] Compose fuer Windows, Linux und macOS vorbereitet.
- [x] Compose fuer x86_64/amd64 und ARM64 vorbereitet.
- [x] Fuer Mac mit M-Chip keine SQL-Server-Emulation mehr noetig; PostgreSQL laeuft nativ auf ARM64.
- [x] Start mit `docker compose up -d` vorbereitet.
- [x] One-click Startdateien fuer Windows, macOS und Linux bereitgestellt.
- [x] Startdateien starten direkt, wenn eine `.env` existiert.
- [x] Startdateien fragen notwendige Auth0/OpenWeatherMap-Werte ab, wenn noch keine `.env` existiert.
- [x] Optionales ngrok vorbereitet.
- [x] ngrok kann ueber `.env`/Compose-Profil aktiviert werden.
- [x] ngrok wartet auf den Frontend-Healthcheck.
- [x] React Dev Server fuer ngrok-Hostzugriff angepasst.

## Umgebungsvariablen und Secrets

- [x] Root-`.env.example` fuer alle Services vervollstaendigt.
- [x] Root-`.env` lokal angelegt, aber nicht in Git aufgenommen.
- [x] Keine echten Keys ins Repo aufgenommen.
- [x] Auth0 Domain aufgenommen.
- [x] Auth0 Audience aufgenommen.
- [x] Auth0 ClientId aufgenommen.
- [x] Auth0 Connection-Namen fuer Database, Google, Apple, Facebook und GitHub vorbereitet.
- [x] OpenWeatherMap API Key aufgenommen.
- [x] PostgreSQL DB Name aufgenommen.
- [x] PostgreSQL User aufgenommen.
- [x] PostgreSQL Passwort aufgenommen.
- [x] pgAdmin Login-Variablen aufgenommen.
- [x] Connection String ueber Compose/Environment vorbereitet.
- [x] Option `LOGS` aufgenommen: `false` erzeugt keinen Log-Ordner, `true` erzeugt ausfuehrliche Backend-Datei-Logs.
- [x] Optionales `NGROK_AUTHTOKEN` und `NGROK_URL` vorbereitet.
- [x] Harte API-Key-Validation fuer OpenWeatherMap vorbereitet.
- [x] Echte ngrok-URL aus Repository-Defaults entfernt.
- [x] Echte Auth0-Tenant-Beispiele aus Repo-Dokumentation bereinigt.

## Auth0, JWT und OAuth

- [x] Auth0 als zentrale Benutzerverwaltung genutzt.
- [x] Keine eigenen Passwort-Hashes in der Projekt-Datenbank gespeichert.
- [x] Benutzername/Passwort ueber Auth0 Database Connection vorbereitet.
- [x] Social Login ueber Auth0 vorbereitet.
- [x] Google Connection vorbereitet.
- [x] Apple Connection vorbereitet.
- [x] Facebook Connection vorbereitet.
- [x] GitHub Connection vorbereitet.
- [x] Instagram aus GUI, Setup und Code entfernt.
- [x] ASP.NET Core JWT Bearer Authentication als C#-Variante zu OAuth2PasswordBearer genutzt.
- [x] `Authorization: Bearer <token>` fuer geschuetzte Endpunkte verwendet.
- [x] `/my-profile` als geschuetzten Profil-Endpunkt bereitgestellt.
- [x] Auth0 Universal Login fuer Login und Registrierung verwendet.
- [x] Passwoerter, Salt und Hashing liegen bei Auth0, nicht in unserer Datenbank.
- [x] Auth0 Rollen/Permissions dokumentiert: `region:all`, `region:eu`, Standardzugriff Deutschland.
- [x] Codebezug dokumentiert: `RegionAuthorization` nutzt Auth0 Permissions.
- [x] Teilen/Freigeben von Wetterstationen mit Auth0-Nutzern konzeptionell beschrieben.

## Datenbank und ORM

- [x] Passende PostgreSQL/ORM-Pakete hinzugefuegt.
- [x] Die 3 Normalformen fuer das Datenmodell beachtet.
- [x] `WeatherDbContext` erstellt.
- [x] Connection String ueber Environment Variable geladen.
- [x] Tabelle `UserProfiles` fuer lokale Auth0-User-Metadaten erstellt.
- [x] Tabelle `SearchHistory` geplant und erstellt.
- [x] Tabelle `FavoriteCities` geplant und erstellt.
- [x] Tabelle `WeatherRequestLog` geplant und erstellt.
- [x] Tabelle `Cities` fuer normalisierte Orte erstellt.
- [x] Tabelle `WeatherStations` fuer eigene Orte/Wetterstationen erstellt.
- [x] Tabelle `WeatherStationMeasurements` fuer eigene Stationsdaten erstellt.
- [x] Tabelle fuer Wetterstation-Freigaben vorbereitet.
- [x] Migrationen erstellt.
- [x] Datenbank beim Start automatisch migriert.
- [x] ORM-Zugriff ueber EF Core verwendet, keine handgeschriebenen SQL-Queries im App-Code.
- [x] `AppUsers` in `UserProfiles` umbenannt.
- [x] Neue Tabelle fuer GUI-Theme pro Nutzer erstellt.
- [x] Migration fuer Theme-Tabelle erstellt.
- [x] Pruefung der Normalformen fuer neue Tabellen begonnen.

## Backend-Endpunkte

- [x] `GET /weather`.
- [x] `GET /uv`.
- [x] `GET /airquality`.
- [x] `GET /forecast`.
- [x] `GET /my-profile`.
- [x] `GET /stations`.
- [x] `POST /stations`.
- [x] `GET /stations/{stationId}/measurements`.
- [x] `POST /stations/{stationId}/measurements`.
- [x] `PUT /stations/{stationId}`.
- [x] `DELETE /stations/{stationId}`.
- [x] `GET /history`.
- [x] `POST /history`.
- [x] `DELETE /history/{id}`.
- [x] `GET /favorites`.
- [x] `POST /favorites`.
- [x] `PUT /favorites/{id}`.
- [x] `DELETE /favorites/{id}`.
- [x] `GET /theme`.
- [x] `PUT /theme`.
- [x] Erfolgreiche Wetterabfragen speichern Suchhistorie automatisch.
- [x] Auth0 User-ID aus JWT genutzt, damit Historie pro Nutzer getrennt ist.
- [x] Favoriten pro Auth0 User getrennt gespeichert.
- [x] Wetterstationen pro Auth0 User getrennt gespeichert.
- [x] Theme pro Auth0 User getrennt gespeichert.
- [x] Saubere REST-API mit passenden HTTP-Methoden und Statuscodes begonnen.
- [x] Fehler 400 fuer ungueltige Eingaben.
- [x] Fehler 401 fuer fehlenden/ungueltigen Login.
- [x] Fehler 403 fuer fehlende Berechtigung.
- [x] Fehler 404 fuer nicht gefundene Staedte oder Ressourcen.
- [x] Fehler 409 fuer Duplikate.
- [x] Fehler 429 fuer Rate Limit.
- [x] Rate Limiting fuer REST-APIs: 100 Requests pro Minute.
- [x] Rate Limiting fuer REST-APIs: 1000 Requests pro Stunde.
- [x] Validierung fuer Stations- und Messwert-Requests eingebaut.
- [x] Keine SQL-Injection im normalen App-Code: EF Core mit parametrisierten ORM-Queries verwendet.

## Frontend

- [x] Komponentenbasierte Struktur vorhanden.
- [x] Daten werden dynamisch von Backend-API geladen.
- [x] Auth0 Login/Logout in der Weboberflaeche angeboten.
- [x] Google, Apple, Facebook und GitHub Login-Schaltflaechen angezeigt.
- [x] Instagram aus der GUI entfernt.
- [x] Eigene Orte/Wetterstationen angezeigt.
- [x] Eigene Wetterstationen anlegen.
- [x] Messwerte fuer Wetterstationen eintragen.
- [x] Messwerte fuer Wetterstationen anzeigen.
- [x] Suchhistorie anzeigen.
- [x] Klick auf alte Suche laedt Wetter erneut.
- [x] Favoriten anzeigen.
- [x] Favoriten anlegen.
- [x] Favoriten aendern.
- [x] Favoriten loeschen.
- [x] Fehlerfall 401 anzeigen: nicht eingeloggt.
- [x] Fehlerfall 403 anzeigen: keine Berechtigung.
- [x] Fehlerfall 404 anzeigen: Stadt nicht gefunden.
- [x] Fehlerfall 409 anzeigen: Duplikat.
- [x] Fehlerfall 429 anzeigen: zu viele Anfragen.
- [x] Fehlerfall 500 anzeigen: Serverfehler.
- [x] GUI-Theme nach Farben umstellen.
- [x] GUI-Theme aus der Datenbank laden.
- [x] GUI-Theme in der Datenbank speichern.
- [x] Aeussere App-Ecken eckig gemacht, innere runde Elemente beibehalten.
- [x] Layout so angepasst, dass nichts ueberlappt.
- [x] Wetterstationsfelder gelockert: nicht alles Pflicht.
- [x] Klare optische Abgrenzung zwischen Wetterdashboard, Verlauf, Favoriten und Wetterstationen.
- [x] Navigation/Routing geprueft: kein separates Routing noetig, im README begruendet.
- [x] Box neben den Favoriten zeigt Freigabe-Verwaltung fuer Wetterstationen statt Suchverlauf.
- [x] Logout/Login-Texte und Wettertexte auf Deutsch verbessert.
- [x] Suchvorschlaege erweitert.
- [x] Default-Stadt zentralisiert.
- [x] Tageswechsel im Frontend aktualisiert sich mit.
- [x] Temperaturkonvertierung vereinheitlicht.
- [x] Wettericons fuer Drizzle/Bewoelkung korrigiert.

## Eigene Orte, Favoriten, Verlauf und Freigaben

- [x] Eigene Wetterstationen sind einem Auth0-Nutzer zugeordnet.
- [x] Eigene Messwerte sind einer Wetterstation zugeordnet.
- [x] Eigene Messwerte werden in PostgreSQL gespeichert.
- [x] Suchverlauf wird automatisch bei erfolgreicher Wetterabfrage gespeichert.
- [x] Suchverlauf kann ueber die GUI erneut geladen werden.
- [x] Suchverlauf kann geloescht werden.
- [x] Favoriten koennen erstellt werden.
- [x] Favoriten koennen angezeigt werden.
- [x] Favoriten koennen geaendert werden.
- [x] Favoriten koennen geloescht werden.
- [x] Favoriten koennen per Klick als aktuelle Wetterstadt geladen werden.
- [x] Wetterstationen koennen geaendert werden.
- [x] Wetterstationen koennen geloescht werden.
- [x] Wetterstationen koennen an andere Nutzer freigegeben werden.
- [x] Freigabe-Workflow geplant: Nutzer A gibt Station an Nutzer B frei.
- [x] Freigabe-Workflow geplant: Nutzer B darf Messwerte eintragen, aber Station nicht besitzen.
- [x] Freigabe-Workflow geplant: Vertretung fuer Messwerterfassung, nicht nur Urlaubsvertretung.
- [x] Datenmodell fuer Freigaben geplant, z. B. `WeatherStationShares`.
- [x] Auth0-User-Suche oder Einladungskonzept fuer Freigaben geplant.

## README und Dokumentation

- [x] Setup mit `docker compose up` dokumentiert.
- [x] Startdateien fuer Windows, macOS und Linux dokumentiert.
- [x] Environment Variablen erklaert.
- [x] Auth0 Setup dokumentiert.
- [x] OpenWeatherMap API Key dokumentiert.
- [x] PostgreSQL und pgAdmin dokumentiert.
- [x] Optionale Detail-Logs dokumentiert.
- [x] Optionales ngrok-Profil dokumentiert.
- [x] EF-Core-Migrationshistory-Hinweis dokumentiert.
- [x] Architekturdiagramm als Mermaid aufgenommen.
- [x] React Frontend im Diagramm gezeigt.
- [x] ASP.NET Core Backend im Diagramm gezeigt.
- [x] PostgreSQL im Diagramm gezeigt.
- [x] OpenWeatherMap im Diagramm gezeigt.
- [x] Auth0 im Diagramm gezeigt.
- [x] Neue Endpunkte fuer Verlauf, Favoriten und Theme dokumentiert.
- [x] Navigation/Routing im README begruendet.
- [x] OpenWeatherMap-Endpunkte dokumentiert.
- [x] ngrok und API-Proxy ueber relative Requests dokumentiert.

## Tests und Pruefungen, die bereits gelaufen sind

- [x] Google Login getestet.
- [x] Apple Login getestet.
- [x] Facebook Login getestet.
- [x] GitHub Login getestet.
- [x] Wetterabfrage getestet.
- [x] Theme/Datenbank getestet.
- [x] Docker Compose Komplettstart getestet.
- [x] Backend Build zuletzt erfolgreich.
- [x] Backend Tests zuletzt erfolgreich.
- [x] Frontend Build zuletzt erfolgreich.
- [x] `git diff --check` zuletzt sauber.

## Abgabe-Artefakte begonnen/fertig

- [x] `README.md` finaler Entwurf.
- [x] `.env.example`.
- [x] `docker-compose.yml`.
- [x] Architekturdiagramm angefangen.
- [x] OAuth via Auth0 Social Logins vorbereitet.
- [x] Google OAuth ueber Auth0.
- [x] Apple OAuth ueber Auth0.
- [x] Facebook OAuth ueber Auth0.
- [x] GitHub OAuth ueber Auth0.

## KI-Review-Punkte, die bereits erledigt wirken

- [x] Keine echten OpenWeatherMap-Keys im Frontend bewusst vorgesehen.
- [x] OpenWeatherMap-Key wird serverseitig ueber Backend genutzt.
- [x] `.env` wird ignoriert.
- [x] `.env.example` nutzt Platzhalter statt echte Secrets.
- [x] Auth0 PKCE/SPA-Ansatz ohne Client Secret im Frontend dokumentiert.
- [x] JWT Bearer Authentication im Backend vorhanden.
- [x] EF Core ORM statt direkter SQL-Strings im normalen App-Code.
- [x] DB-Schema normalisiert mit zentralen Cities und UserProfiles.
- [x] Suchverlauf-Trim auf 3 Eintraege umgesetzt.
- [x] Startup-Skripte fuer Windows, PowerShell und Shell vorhanden.
- [x] OpenWeatherMap Forecast von altem Daily-Endpunkt auf 5-Tage/3-Stunden-Forecast umgestellt.
- [x] UV von altem `/data/2.5/uvi` auf One Call 3.0 umgestellt.
- [x] Forecast-Tagesnamen auf deutsche Ausgabe angepasst.
- [x] `VisibilityKm` im Raw-OpenWeather-Modell in `VisibilityMeters` umbenannt.
- [x] `currentDay` aktualisiert sich jetzt mit dem Timer.
- [x] `convertTemperature` gibt konsistent Zahlen zurueck.
- [x] Doppelte Cloud-/Drizzle-Icon-Zuordnung korrigiert.


## Aus TODO verschoben am 29.05.2026

- [x] Kompletten frischen Start testen: `docker compose down`, dann `docker compose up -d`.
- [x] Testen, dass Frontend, Backend, DB, pgAdmin und optional ngrok gemeinsam starten.
- [x] Pruefen, dass nach neuen Features keine Secrets in Git landen.
- [x] Pruefen, ob `docker compose up -d` mit gefuelltem ngrok-Setup ngrok startet.
- [x] Pruefen, ob Compose mit leerem ngrok-Setup ohne ngrok geplant wird.
- [x] Nochmals pruefen, ob im README keine echte Auth0-Tenant-ID mehr steht.
- [x] Nochmals pruefen, ob in `.env.example`, `docker-compose.yml`, Start-Skripten und README keine echte ngrok-URL mehr als Default steht.
- [x] Secret-Scan fuer komplette Git-Historie durchfuehren.
- [x] Pruefen, ob alte Commits jemals echte `.env`-Werte, OpenWeatherMap-Keys, Auth0-Werte oder ngrok-Tokens enthalten haben.
- [x] `.env.example` auf unsichere Default-Passwoerter pruefen.
- [x] Start-Skripte sollen bei Default-Passwoertern wie `change-me-for-local-development` warnen oder abbrechen.
- [x] Auth0 Domain, Audience und ClientId auf plausibles Format validieren.
- [x] OpenWeatherMap API Key beim Start hart validieren.
- [x] `start-weather-app.ps1` pruefen: Docker installiert?
- [x] `start-weather-app.ps1` pruefen: Docker Daemon laeuft?
- [x] `start-weather-app.ps1` pruefen: Ports 3000, 5122, 5432, 5050 frei?
- [x] `start-weather-app.ps1` pruefen: Pflichtwerte in bestehender `.env` gesetzt?
- [x] `start-weather-app.ps1` pruefen: Auth0 Domain Format gueltig?
- [x] `start-weather-app.ps1` pruefen: Auth0 Audience plausibel?
- [x] `start-weather-app.ps1` pruefen: OpenWeatherMap Key nicht leer/Placeholder?
- [x] `start-weather-app.ps1` pruefen: ngrok URL gueltig, falls gesetzt?
- [x] `start-weather-app.ps1` soll `.env` atomar schreiben, erst `.env.tmp`, dann rename.
- [x] `start-weather-app.ps1` soll Backup erstellen, bevor bestehende `.env` ueberschrieben wird.
- [x] `start-weather-app.ps1` soll Docker-Fehler klar anzeigen.
- [x] `start-weather-app.sh` mit `set -euo pipefail` pruefen.
- [x] `start-weather-app.sh` Docker installiert/Daemon laeuft pruefen.
- [x] `start-weather-app.sh` `docker compose` vs `docker-compose` fallback pruefen.
- [x] `start-weather-app.sh` `openssl` Fallback fuer Passwortgenerierung pruefen.
- [x] `start-weather-app.sh` Trap fuer `stty echo` bei Abbruch einbauen.
- [x] `start-weather-app.sh` `.env` atomar schreiben.
- [x] `start-weather-app.sh` `.env` Backup vor Ueberschreiben erstellen.
- [x] `start-weather-app.sh` Ausfuehrbarkeit und LF-Zeilenenden dokumentieren.
- [x] `start-weather-app.bat` pruefen: `chcp 65001` fuer Umlaute setzen.
- [x] `start-weather-app.bat` Docker-Verfuegbarkeit pruefen oder klar nur Wrapper lassen.
- [x] Start-Skripte sollen `.env` nicht neu schreiben, wenn sie bereits existiert und gueltig ist.
- [x] Start-Skripte sollen bei bestehender `.env` optional nur `docker compose up -d` ausfuehren.
- [x] Separate Passwoerter fuer PostgreSQL und pgAdmin generieren.
- [x] Optionale Flags pruefen: `--with-ngrok`, `--validate-only`.
- [x] Post-Setup-Checks ergaenzen: Frontend erreichbar, Backend Swagger/Health-Proxy erreichbar, DB Health ueber Compose.
