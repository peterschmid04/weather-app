# Weather App mögliches TODO

Stand: 29.05.2026

Hinweis: Diese Datei ist nur eine Sammelliste. Die Punkte aus externen KI-Reviews sind hier bewusst dokumentiert, aber noch nicht bewertet und noch nicht umgesetzt. Manche Punkte koennen falsch, veraltet, doppelt oder fuer dieses Projekt nicht sinnvoll sein. Spaeter Punkt fuer Punkt pruefen und erst danach entscheiden.

## Kurz-Zusammenfassung

- Das Projekt ist als Prototyp schon weit: React-Frontend, ASP.NET-Core-Backend, PostgreSQL, pgAdmin, Docker Compose, Auth0 und OpenWeatherMap sind vorhanden.
- Die wichtigsten offenen Themen sind Wartbarkeit, Tests, Produktionsnahe Docker-Images, robuste Setup-Skripte, bessere Validierung, bessere Fehlerbehandlung, Caching, CI/CD und Dokumentationsaufteilung.
- Mehrere Review-Punkte wurden laut aktuellem Stand bereits erledigt und stehen deshalb in `WEATHER_APP_DONE.md`.
- Diese Todo-Datei enthaelt nur offene oder noch zu pruefende Punkte.

## Sofort Pruefen

- [ ] Pruefen: frischer Clone plus lokale `.env` plus `docker compose up -d` reicht wirklich.
- [ ] Responsive Darstellung mit Stationen, Favoriten, Verlauf, Freigaben und Themes visuell pruefen.
- [ ] Fachliche Funktionen komplett durchtesten: Wetterdashboard, Stadtsuche, Forecast, UV-Index, Luftqualitaet, Login, Verlauf, Favoriten, Wetterstationen, Themes.
- [ ] Fehlerfaelle 400, 401, 403, 404, 409, 429 und 500 im Frontend und Backend testen.

## Security und Secrets

- Ergebnis 29.05.2026: Aktueller Stand enthaelt keine Treffer fuer die alte echte Auth0-Tenant-ID oder echte ngrok-URL in README, `.env.example`, `docker-compose.yml` oder Start-Skripten; die Git-Historie enthaelt aber noch Treffer fuer alte Auth0/ngrok-Werte und muss als potenziell exponiert behandelt werden.
- [ ] Falls alte Secrets in Git-Historie waren: Keys/Tokens rotieren und Repo-Historie bereinigen oder Risiko dokumentieren.
- [ ] Pruefen, ob Auth0 Client Secret wirklich nicht gebraucht wird; fuer SPA/JWT normalerweise nicht ins Projekt schreiben.
- [ ] Dokumentieren, dass Auth0 ClientId und Audience in einer SPA oeffentlich lesbar sind und keine Secrets sind.
- [ ] `DANGEROUSLY_DISABLE_HOST_CHECK=true` bewerten und nur fuer lokale ngrok-Entwicklung erlauben.
- [ ] Alternative zu `DANGEROUSLY_DISABLE_HOST_CHECK` pruefen, z. B. explizite Host-Konfiguration oder Reverse Proxy.
- [ ] Token-Storage im Frontend pruefen: Local Storage Risiko bei XSS bewerten.
- [ ] Content Security Policy fuer Frontend planen.
- [ ] Security Headers fuer Frontend/Backend pruefen.
- [ ] Swagger nur in Development aktivieren oder schuetzen.
- [ ] pgAdmin und PostgreSQL-Ports nicht unnoetig nach aussen freigeben.
- [ ] PostgreSQL-Port optional nur an `127.0.0.1` binden oder gar nicht exposen.
- [ ] pgAdmin-Port optional nur an `127.0.0.1` binden.
- [ ] CORS strikt konfigurieren und erlaubte Origins aus Konfiguration lesen.
- [ ] CORS nicht unnoetig `http://localhost:5122` erlauben.
- [ ] CORS Methods und Headers enger whitelisten, z. B. `GET`, `POST`, `PUT`, `DELETE`, `Authorization`, `Content-Type`, `Accept`.
- [ ] CORS `Fun-Text` Header nur exponieren, wenn dieser Header wirklich gebraucht wird.
- [ ] SQL-Injection-Risiko pruefen: keine unsicheren `FromSqlRaw`-Strings, nur EF Core/parametrisierte Queries.
- [ ] Docker/ngrok Sicherheitsrisiken im README deutlicher markieren.
- [ ] `.env` niemals in Images kopieren; fuer Production Docker Secrets oder Plattform-Umgebungsvariablen dokumentieren.

## Backend Architektur und Clean Code

- [ ] `Program.cs` aufteilen; aktuelle Minimal-API-Datei ist zu gross und zu schwer wartbar.
- [ ] Endpunkte in Feature-Dateien auslagern, z. B. `WeatherEndpoints`, `HistoryEndpoints`, `FavoritesEndpoints`, `StationsEndpoints`, `SharesEndpoints`, `ThemeEndpoints`, `AccessEndpoints`.
- [ ] Hilfsmethoden aus `Program.cs` in Services/Helper-Klassen verschieben.
- [ ] Validierungen aus `Program.cs` in Validatoren auslagern.
- [ ] Pruefen, ob Clean Architecture mit `Domain`, `Application`, `Infrastructure`, `API` sinnvoll ist.
- [ ] Alternativ Feature-Folder-Struktur einfuehren: `Features/Weather`, `Features/Stations`, `Features/Favorites`, `Features/Shares`.
- [ ] Repository-Pattern oder Service-Layer fuer groessere Endpunkte pruefen.
- [ ] Datenbankzugriffe aus Endpunkten in Services verschieben.
- [ ] `GetOrCreateCurrentUserAsync` caching-faehig machen, z. B. mit `IMemoryCache` pro Auth0-Subject.
- [ ] `FindOrCreateCityAsync` Race Condition absichern.
- [ ] Unique-Constraint-Fehler bei parallelem City-Create sauber abfangen.
- [ ] Mehrere `SaveChangesAsync` pro Request reduzieren.
- [ ] Transaktionen fuer zusammenhaengende DB-Aenderungen pruefen.
- [ ] `WeatherRequestLogs` begrenzen oder Retention einbauen, z. B. letzte 100 pro User oder aelter als 30 Tage loeschen.
- [ ] Cleanup-Job fuer ungenutzte Cities pruefen.
- [ ] `SearchHistory` Limit 3 konfigurierbar machen oder fachlich dokumentieren.
- [ ] Magic Numbers und Magic Strings durch Konstanten oder Optionen ersetzen.
- [ ] Permissions wie `region:all`, `region:eu` als Konstanten pflegen.
- [ ] Share-Status wie `pending`, `accepted` als Enum modellieren.
- [ ] Share-Permissions wie `read`, `write_measurements` als Enum modellieren.
- [ ] Theme-Namen als Konstanten/Enum pflegen.
- [ ] RegionAuthorization nicht hart statisch halten oder zumindest besser dokumentieren.
- [ ] Regionen und Laenderlisten konfigurierbar machen oder bewusst als statischen Prototyp begruenden.
- [ ] `Random.Shared` statt eigener `Random`-Instanz fuer Middleware/Header verwenden.
- [ ] Fun-Text/Easter-Egg-Header entfernen oder hinter Dev-Flag setzen.
- [ ] `AddAuthorization` doppelte Registrierung pruefen und redundante Registrierung entfernen.
- [ ] `UseHttpsRedirection()` nur fuer Production aktivieren oder im HTTP-Container entfernen.
- [ ] `ASPNETCORE_URLS` und `--urls` Duplikat entfernen.
- [ ] Swagger-Routen mit Auth/FallbackPolicy bewusst konfigurieren.
- [ ] FallbackPolicy und Swagger/Health-Endpunkte sauber voneinander trennen.
- [ ] Globales Exception Handling ueber Middleware einbauen.
- [ ] ProblemDetails fuer Fehlerantworten konsistent verwenden.
- [ ] Fehler 502/503 fuer externe OpenWeatherMap-Probleme pruefen.
- [ ] Keine internen Exception-Details an Clients leaken.
- [ ] Lat/Lon-Validierung fuer `/uv`, `/airquality`, `/forecast` ergaenzen.
- [ ] Region-Pruefung auch fuer Koordinaten-Endpunkte `/uv`, `/airquality`, `/forecast` durchsetzen.
- [ ] Region-Fehler im Frontend konkreter anzeigen.
- [ ] API-Versionierung pruefen, z. B. `/api/v1/...`.
- [ ] `/healthz` Endpoint ergaenzen.
- [ ] ASP.NET Core HealthChecks einbauen.
- [ ] `ApiKeyMiddleware.cs` pruefen: entweder aktiv nutzen oder entfernen.
- [ ] Tote `Auth:ApiKey`-Config entfernen, falls ApiKeyMiddleware nicht genutzt wird.
- [ ] `WeatherDbContextFactory` Fallback-Connectionstring mit `change-me` pruefen und absichern.
- [ ] `NormalizedSharedWithEmail` Normalisierung pruefen, ggf. Lowercase statt Uppercase.
- [ ] Share-Akzeptanz per E-Mail auf Edge-Cases pruefen.
- [ ] Ablaufdatum fuer Einladungen/Freigaben pruefen.
- [ ] DSGVO-Loeschkonzept fuer UserProfile, Logs, History und Shares dokumentieren.

## Backend OpenWeatherMap und externe APIs

- [ ] Aktuelles Wetter direkt mit `units=metric` abrufen, statt Kelvin manuell umzurechnen.
- [ ] OpenWeatherMap Base-URLs zentral konfigurieren.
- [ ] Typed/Named `HttpClient` fuer OpenWeatherMap erstellen.
- [ ] Timeout fuer externe API-Calls setzen.
- [ ] Retry-Policy fuer transiente OpenWeatherMap-Fehler pruefen, z. B. Polly.
- [ ] Circuit Breaker fuer externe API-Ausfaelle pruefen.
- [ ] OpenWeatherMap-Antworten defensiver deserialisieren.
- [ ] `GetAirQuality` nicht blind `.First()` auf leere Listen ausfuehren.
- [ ] Koordinaten nicht auf zwei Nachkommastellen begrenzen, wenn Wetterstationen genauer sind.
- [ ] Wind-Einheit pruefen: OpenWeatherMap liefert bei Metric m/s, Frontend zeigt teilweise km/h.
- [ ] Wind in Backend zu km/h konvertieren oder Frontend-Label auf m/s aendern.
- [ ] OpenWeatherMap Geocoding API pruefen, damit Alias-Liste fuer Freiburg/Muenchen/Wien nicht beliebig waechst.
- [ ] City-Autocomplete langfristig ueber Geocoding/API oder Datenbank statt harter Liste pruefen.
- [ ] Caching fuer Wetterdaten einbauen, z. B. 5 bis 10 Minuten per `IMemoryCache`.
- [ ] OpenWeatherMap Rate Limits im Backend beruecksichtigen.
- [ ] UI-Hinweis anzeigen, wenn One Call 3.0 fuer UV im OpenWeatherMap-Konto nicht aktiviert ist.

## Backend Tests

- [ ] Unit-Tests fuer `RegionAuthorization` schreiben.
- [ ] Unit-Tests fuer Validierungsmethoden schreiben.
- [ ] Unit-Tests fuer `WeatherService`, `ForecastService`, `UvService`, `AirQualityService` erweitern.
- [ ] Tests fuer OpenWeatherMap Fehlerantworten schreiben.
- [ ] Tests fuer leere oder kaputte OpenWeatherMap-Antworten schreiben.
- [ ] Integrationstests fuer Endpunkte mit `WebApplicationFactory` schreiben.
- [ ] Integrationstests fuer Auth-Pflicht schreiben.
- [ ] Integrationstests fuer 401, 403, 404, 409 und 429 schreiben.
- [ ] Tests fuer doppelte Favoriten schreiben.
- [ ] Tests fuer Suchhistorie-Limit schreiben.
- [ ] Tests fuer Station-CRUD schreiben.
- [ ] Tests fuer Station-Measurements schreiben.
- [ ] Tests fuer Station-Sharing schreiben.
- [ ] Tests fuer Race Condition bei City-Create pruefen.
- [ ] Leere Testdateien entfernen, falls vorhanden.
- [ ] Test-Coverage-Ziel definieren.

## Frontend Architektur und Clean Code

- [ ] Migration von Create React App zu Vite pruefen.
- [ ] Migration von JavaScript zu TypeScript planen.
- [ ] API-Response-Typen fuer Weather, Forecast, UV, AirQuality, Favorites, History, Stations, Shares und Theme definieren.
- [ ] Grosse `App.js` in kleinere Komponenten/Hooks aufteilen.
- [ ] `UserDataPanel.js` in `FavoritesManager`, `SearchHistory`, `ShareManager`, `AccessOverview`, `ThemeSelector` aufteilen.
- [ ] `Stations.js` in Station-CRUD und Measurement-CRUD aufteilen.
- [ ] Feature-Folder-Struktur pruefen, z. B. `features/weather`, `features/stations`, `features/auth`.
- [ ] `authFetchJson` nicht mehr per Props durchreichen; Context oder API-Client nutzen.
- [ ] Zentralen `apiClient` erstellen.
- [ ] API Base URL nur an einer Stelle pflegen.
- [ ] Pruefen, ob `axios` genutzt oder entfernt werden soll.
- [ ] Falls `axios` bleibt: Interceptors fuer Auth-Header und Fehler nutzen.
- [ ] Falls `fetch` bleibt: ungenutzte `axios` Dependency entfernen.
- [ ] React Query/TanStack Query fuer Server-State pruefen.
- [ ] Manuelle Refresh-Keys durch Query Invalidation ersetzen.
- [ ] Loading-States fuer alle API-Aktionen anzeigen.
- [ ] Empty-States fuer Verlauf, Favoriten, Stationen und Messwerte anzeigen.
- [ ] Error Boundary fuer React-Tree einbauen.
- [ ] Fehlertexte zentralisieren.
- [ ] Teilfehler anzeigen, wenn UV, Forecast oder Luftqualitaet fehlt, Wetter aber geladen wurde.
- [ ] Netzwerkfehler und Backend-Fehler sprachlich konsistent anzeigen.
- [ ] Search-Form semantisch nur um Suchbereich legen, nicht um komplette Sidebar.
- [ ] Suchvorschlaege langfristig performanter machen.
- [ ] Suche debouncen.
- [ ] City-Vorschlaege nur passend zum Prefix anzeigen.
- [ ] Keine Autokorrektur in Suchfeld, wenn User bewusst etwas eingegeben hat.
- [ ] Formularvalidierung im Frontend ergaenzen, z. B. CountryCode Pattern, Koordinaten min/max, Messwerte min/max.
- [ ] Key-Props nicht mit Index setzen, wenn Listen dynamisch sind.
- [ ] `convertTemperature` zentral in Utils auslagern.
- [ ] Status-/Weather-Utils von `switch(true)` auf klarere if/else oder Mapping-Struktur pruefen.
- [ ] Magic Numbers im Frontend als Konstanten definieren, z. B. UV-Winkel, Wind-Stufen, Humidity-Stufen.
- [ ] `getStatusVisibility` Semantik pruefen.
- [ ] Uhrzeit-Intervall so verbessern, dass Minutenwechsel genauer angezeigt wird.
- [ ] `window.location.origin` fuer Tests mockbar machen.
- [ ] `catch (_)` vermeiden oder Fehler sinnvoll loggen.
- [ ] `searchMessage` und `error` States konsolidieren.
- [ ] Alt-Texte fuer Wettericons aussagekraeftiger machen.
- [ ] ARIA-Labels und semantisches HTML pruefen.
- [ ] Keyboard-Navigation pruefen.
- [ ] Accessibility-Audit planen.
- [ ] i18n/Mehrsprachigkeit mit `react-i18next` als spaetere Option dokumentieren.
- [ ] Code-Splitting mit `React.lazy` und `Suspense` pruefen.
- [ ] Image-Optimierung fuer Assets pruefen.
- [ ] Frontend-Tests mit React Testing Library oder Vitest einbauen.
- [ ] E2E-Tests mit Playwright oder Cypress planen.

## Frontend Metadaten und Dependencies

- [ ] `cra-template` aus Dependencies entfernen, falls noch vorhanden.
- [ ] Ungenutztes Tailwind/PostCSS/Autoprefixer pruefen und entfernen oder richtig konfigurieren.
- [ ] `weather-app/README.md` im Frontend-Unterordner pruefen; falls CRA-Default, loeschen oder projektspezifisch ersetzen.
- [ ] `manifest.json` auf App-Namen, Short-Name und Icons pruefen.
- [ ] `public/index.html` auf `lang="de"`, Meta Description, Theme Color und Open Graph Tags pruefen.
- [ ] `eject` Script entfernen, falls nicht gebraucht.
- [ ] React StrictMode pruefen.
- [ ] Browserslist/caniuse-lite Update-Strategie dokumentieren.
- [ ] Dependency Audit mit `npm audit` oder GitHub Dependabot planen.
- [ ] ESLint/Prettier konfigurieren.
- [ ] Husky/lint-staged als optionalen Pre-Commit-Check pruefen.

## Docker und Infrastruktur

- [ ] Docker Images pinnen, statt `latest` zu verwenden.
- [ ] PostgreSQL-Version bewusst festlegen, z. B. `postgres:17` oder anderer getesteter Tag.
- [ ] pgAdmin-Version bewusst festlegen.
- [ ] ngrok-Version bewusst festlegen oder Risiko von `latest` dokumentieren.
- [ ] Node-Version bewusst festlegen.
- [ ] Pruefen, ob `node:24` fuer Zielzeitpunkt/Umfeld stabil ist.
- [ ] PostgreSQL Volume-Pfad pruefen und an gepinnten Tag anpassen.
- [ ] Fuer `postgres:17` Datenpfad `/var/lib/postgresql/data` pruefen.
- [ ] Datenverlust-Risiko bei falschem PostgreSQL-Volume-Pfad testen und dokumentieren.
- [ ] Backend-Healthcheck ergaenzen.
- [ ] pgAdmin-Healthcheck ergaenzen.
- [ ] ngrok-Healthcheck pruefen.
- [ ] Frontend-Healthcheck-Timeout realistisch setzen.
- [ ] Dedicated `/healthz` Endpoint fuer Backend.
- [ ] `depends_on` fuer Frontend auf Backend-Health statt nur Container-Start pruefen.
- [ ] Multi-Stage Dockerfile fuer Backend erstellen.
- [ ] Backend Runtime-Image mit `aspnet` statt dauerhaft SDK fuer Production pruefen.
- [ ] Multi-Stage Dockerfile fuer Frontend erstellen.
- [ ] Frontend Production-Build mit Nginx oder anderem Static Server pruefen.
- [ ] Entwicklungs-Compose und Production-naeheres Compose trennen.
- [ ] `.dockerignore` erstellen oder pruefen.
- [ ] Container als non-root User laufen lassen.
- [ ] Docker Resource Limits fuer DB/Backend/Frontend pruefen.
- [ ] `dotnet restore` nicht bei jedem Start neu ausfuehren muessen, fuer Production ins Image verlagern.
- [ ] `npm ci` nicht bei jedem Start neu ausfuehren muessen, fuer Production ins Image verlagern.
- [ ] NuGet- und node_modules-Volumes weiter fuer Development dokumentieren.
- [ ] Docker-Ordner und Init-Dateien explizit im README als Voraussetzung dokumentieren.
- [ ] `docker compose down -v` Warnung mit Datenverlust-Hinweis prominent halten.
- [ ] Cleanup-Strategie fuer Docker Volumes dokumentieren.
- [ ] Optionales `docker-compose.prod.yml` planen.
- [ ] Optionales HTTPS/lokales TLS fuer produktionsnahes Setup pruefen.

## Start-Skripte

- [ ] Zentrales Setup-Skript statt doppelter Logik in PowerShell und Shell pruefen.
- [ ] Optionales Flag `--reset-db` pruefen; aktuell bewusst nicht automatisiert, damit keine Daten versehentlich geloescht werden.

## Datenbank und ORM

- [ ] Indizes pruefen: `SearchHistory(UserProfileId, CreatedAtUtc)`.
- [ ] Indizes fuer `WeatherStationMeasurement.MeasuredAtUtc` pruefen.
- [ ] Unique-Constraint fuer `WeatherStationShares(StationId, Email)` oder passende Normalform pruefen.
- [ ] Pagination fuer Messwerte einbauen, statt nur `Take(50)`.
- [ ] Pagination fuer groessere Listen pruefen.
- [ ] Soft Deletes fuer bestimmte Daten pruefen.
- [ ] Seed-Data falls sinnvoll planen.
- [ ] Migrationen vor Abgabe pruefen: behalten oder sauber squashing/fresh initial migration?
- [ ] `__EFMigrationsHistory` im README und DB-Setup erklaeren, falls noetig.
- [ ] Datenschutz/DSGVO fuer WeatherRequestLogs und UserProfile dokumentieren.
- [ ] App-Datenbank speichert keine Passwoerter; das weiterhin klar dokumentieren.

## Fachliche Funktionen testen

- [ ] Wetterdashboard zeigt aktuelle Wetterdaten.
- [ ] Stadtsuche funktioniert mit deutschen Umlauten.
- [ ] Stadtsuche funktioniert ohne ungewollte Autokorrektur.
- [ ] Forecast funktioniert mit Free-OpenWeatherMap-Key.
- [ ] UV-Index funktioniert, falls One Call 3.0 aktiviert ist.
- [ ] Luftqualitaet funktioniert.
- [ ] Authentifizierter Zugriff funktioniert.
- [ ] Suchhistorie funktioniert.
- [ ] Suchhistorie speichert hoechstens 3 Eintraege.
- [ ] Suchhistorie loescht ueberschuessige Eintraege auch aus DB.
- [ ] Favoriten funktionieren.
- [ ] Favoriten Duplikate zeigen saubere Meldung.
- [ ] Favoriten letzter Eintrag loeschen wirft keinen Fehler.
- [ ] Favoriten bearbeiten ohne Aenderung wirft keinen Fehler.
- [ ] Eigene Wetterstationen funktionieren.
- [ ] Messwerte fuer eigene Wetterstationen funktionieren.
- [ ] Wetterstationen teilen funktioniert.
- [ ] Vertretung fuer Messwerte funktioniert.
- [ ] Station-Shares annehmen/loeschen funktioniert.
- [ ] GUI-Themes funktionieren.
- [ ] Theme-Wechsel verursacht keinen Runtime-Fehler.
- [ ] ngrok externer Zugriff funktioniert inklusive API-Proxy.
- [ ] Auth0 Login funktioniert lokal.
- [ ] Auth0 Login funktioniert ueber ngrok, wenn Callback/Logout/Web Origins gesetzt sind.

## Bewertung und Abgabe

- [ ] A1: Alle geplanten Features vollstaendig und fehlerfrei pruefen.
- [ ] A1: One-click Start ohne weitere manuelle Eingriffe pruefen.
- [ ] A2: Saubere REST-API mit korrekten HTTP-Methoden und Statuscodes final auditieren.
- [ ] A2: Validierung fuer alle Requests final auditieren.
- [ ] A2: JWT/Auth0 korrekt dokumentieren.
- [ ] A2: Keine sensiblen Daten im Code final pruefen.
- [ ] A3: Komponentenstruktur auditieren.
- [ ] A3: Dynamische API-Daten im Frontend pruefen.
- [ ] A3: Formulare vollstaendig pruefen.
- [ ] A4: Docker Compose Komplettstart final testen.
- [ ] A4: Inter-Container-Kommunikation final testen.
- [ ] A4: Konfiguration ueber Umgebungsvariablen final pruefen.
- [ ] A00: OAuth Social Logins als Bonus dokumentieren.
- [ ] A00: Vollwertige API-Schnittstelle fuer Maschinenkommunikation dokumentieren.
- [ ] A00: Extra Feature Idee Wetterstation teilen final bewerten.
- [ ] A00: Extra Feature Idee Vertretung fuer Messwerte final bewerten.
- [ ] A00: Extra Feature Idee rollenbasierte Admin-/Region-Ansicht final bewerten.

## Dokumentation

- [ ] README in Quickstart und separate Docs aufteilen pruefen.
- [ ] `docs/auth0-setup.md` pruefen.
- [ ] `docs/ngrok.md` pruefen.
- [ ] `docs/database.md` pruefen.
- [ ] `docs/api.md` oder Swagger/OpenAPI Export pruefen.
- [ ] README Troubleshooting erweitern.
- [ ] Auth0 `Callback URL mismatch` Troubleshooting klar halten.
- [ ] OpenWeatherMap One Call / UV Troubleshooting ergaenzen.
- [ ] Docker Compose Startdauer beim ersten Start erklaeren.
- [ ] Docker Logs lesen dokumentieren.
- [ ] Datenbank Reset und Backup dokumentieren.
- [ ] API-Endpunkte mit Beispielen dokumentieren.
- [ ] API fuer Maschinenkommunikation dokumentieren.
- [ ] Swagger-Beispiele ergaenzen.
- [ ] Postman/HTTP-Beispiele ergaenzen.
- [ ] `weatherAPI.http` fuer alle wichtigen Endpunkte erweitern.
- [ ] Architekturdiagramm final pruefen.
- [ ] Testprotokoll final schreiben.
- [ ] Arbeitsnachweis final schreiben.
- [ ] Praesentation erstellen.
- [ ] CONTRIBUTING.md erstellen.
- [ ] CHANGELOG.md erstellen.
- [ ] LICENSE-Datei pruefen oder bewusst weglassen und "All rights reserved" dokumentieren.
- [ ] `.editorconfig` ergaenzen.
- [ ] `.NET global.json` pruefen.
- [ ] Browser-Support dokumentieren.
- [ ] DSGVO/Datenschutzverhalten dokumentieren.
- [ ] Secrets-Rotation dokumentieren.
- [ ] GitHub Issues statt langer Todo-Datei fuer Teamarbeit pruefen.

## CI/CD und Qualitaetssicherung

- [ ] GitHub Actions fuer Backend Build erstellen.
- [ ] GitHub Actions fuer Backend Tests erstellen.
- [ ] GitHub Actions fuer Frontend Install erstellen.
- [ ] GitHub Actions fuer Frontend Build erstellen.
- [ ] GitHub Actions fuer Frontend Tests erstellen, sobald vorhanden.
- [ ] Docker Build in CI pruefen.
- [ ] Secret Scanning in GitHub aktivieren.
- [ ] Dependabot oder Dependency-Updates aktivieren.
- [ ] Format-Check fuer C# pruefen.
- [ ] ESLint-Check fuer Frontend pruefen.
- [ ] Prettier-Check pruefen.
- [ ] Coverage-Report pruefen.
- [ ] Security Audit fuer npm und NuGet pruefen.

## Performance und Skalierung

- [ ] OpenWeatherMap-API-Calls cachen.
- [ ] DB-Queries fuer Listen paginieren.
- [ ] Query-Performance fuer User-Daten pruefen.
- [ ] N+1 Queries bei EF Core pruefen, z. B. `Include` fuer City.
- [ ] Strukturierte Logs mit Correlation ID ergaenzen.
- [ ] Serilog oder anderes strukturiertes Logging pruefen.
- [ ] File-Logger mit Rotation, Max-Dateigroesse und Async-Queue pruefen.
- [ ] Monitoring mit Sentry/Seq/ELK optional pruefen.
- [ ] Metrics mit Prometheus/Grafana optional pruefen.
- [ ] Redis oder Distributed Cache optional pruefen.

## Offene KI-Review-Punkte mit Unsicherheit

- [ ] Pruefen: "node:24 existiert nicht" war evtl. falsch oder zeitabhaengig; aktuelles Docker-Image gezielt verifizieren.
- [ ] Pruefen: "AUTH0_CLIENT_SECRET fehlt" ist fuer SPA/JWT wahrscheinlich falsch; im Projekt nicht unkritisch einfuehren.
- [ ] Pruefen: "keine DI" ist vermutlich teilweise falsch, weil Services und EF Core bereits DI nutzen.
- [ ] Pruefen: "keine async Controller" ist vermutlich ungenau, da Minimal-API-Endpunkte async sein koennen.
- [ ] Pruefen: "keine Swagger-Dokumentation" kann falsch sein, falls Swagger schon aktiv ist.
- [ ] Pruefen: "kein Rate Limiting" kann falsch sein, falls API-Rate-Limiting schon implementiert ist.
- [ ] Pruefen: "API-Key hart im Frontend-Code" klingt nach falscher Analyse; dennoch Secret-Scan machen.
- [ ] Pruefen: "keine Tests implementiert" ist teilweise falsch, weil Backend-Tests vorhanden sind, aber Testabdeckung bleibt offen.
- [ ] Pruefen: "Controller-Ordner" und klassische Controller-Hinweise passen evtl. nicht, da Projekt ASP.NET Minimal API nutzt.
- [ ] Pruefen: "HTTPS lokal erzwingen" fuer Pruefungsprototyp sinnvoll oder zu viel Aufwand?
