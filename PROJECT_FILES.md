# Projekt-Dateien

Diese Datei erklaert die wichtigsten Dateien im Projekt-Root und wie die
einzelnen Teile zusammenarbeiten. Die technische Detaildokumentation liegt
flach an den passenden Stellen:

- `docker/DOCKER_FILES.md`
- `weather-app/FRONTEND_FILES.md`
- `weatherAPIOAuth/BACKEND_FILES.md`
- `weatherAPIOAuth/weatherAPI/API_FILES.md`
- `weatherAPIOAuth/weatherAPI.Test/TEST_FILES.md`

Es gibt bewusst keine tief verschachtelten Dokumentationsordner. Alle neuen
Dokumentationsdateien liegen maximal zwei Projekt-Ebenen tief.

## Root-Dateien

`README.md`

Die zentrale Anleitung fuer das Projekt. Sie beschreibt den Zweck der App,
den Docker-Start, Auth0, OpenWeatherMap, PostgreSQL, pgAdmin, ngrok, Logs und
Testbefehle. Sie ist die erste Datei, die man nach einem frischen Clone liest.

`.env.example`

Die einzige Vorlage fuer lokale Umgebungsvariablen. Echte Werte kommen in eine
lokale `.env`, die nicht committed wird. Die Vorlage enthaelt Platzhalter und
erklaert, welche Werte man aus Auth0, OpenWeatherMap und optional ngrok braucht.

`.gitignore`

Schliesst lokale Secrets, Build-Ausgaben, Abhaengigkeiten, Logs und
Container-Artefakte aus Git aus. Dadurch bleiben `.env`, `node_modules`, `bin`,
`obj`, Build-Ordner und Logdateien lokal.

`docker-compose.yml`

Definiert den lokalen Stack:

- `db`: PostgreSQL
- `pgadmin`: Datenbank-GUI
- `backend`: ASP.NET Core API
- `frontend`: React-App
- `ngrok`: optionaler Tunnel

Alle Services beziehen Konfiguration aus `.env` oder aus sicheren Defaults fuer
lokale Entwicklungswerte.

`start-weather-app.bat`

Windows-CMD-Einstieg. Die Datei ist absichtlich klein und ruft das PowerShell
Setupscript auf. Dadurch liegt die eigentliche Logik nur an einer Stelle.

`start-weather-app.ps1`

Windows-Setupscript. Es prueft Docker, validiert eine bestehende `.env`, legt
bei Bedarf eine neue `.env` atomar an und startet danach Docker Compose.

`start-weather-app.sh`

macOS/Linux-Setupscript. Es hat die gleiche Aufgabe wie die PowerShell-Variante:
Pflichtwerte pruefen, `.env` nur bei Bedarf schreiben und Docker Compose
starten.

`WEATHER_APP_TODO.md`

Offene Aufgaben, Review-Punkte und Ideen. Punkte werden erst abgehakt, wenn sie
wirklich umgesetzt und geprueft sind.

`WEATHER_APP_DONE.md`

Erledigte Aufgaben als Arbeitsnachweis. Diese Datei trennt abgeschlossene Arbeit
von offenen Punkten.

## Laufzeitfluss

1. Docker Compose startet PostgreSQL.
2. PostgreSQL initialisiert bei frischem Volume die EF-Migrationstabelle.
3. Das Backend wartet auf die Datenbank, migriert das Schema und startet die API.
4. Das Frontend startet den React-Dev-Server.
5. Der Browser ruft das Frontend auf Port `3000` auf.
6. Das Frontend sendet API-Anfragen ueber relative Pfade wie `/weather`.
7. Der React-Dev-Server proxyt diese Anfragen intern zum Backend.
8. Das Backend prueft Auth0 Bearer Tokens und ruft OpenWeatherMap serverseitig.
9. Persistente Nutzerdaten landen ueber EF Core in PostgreSQL.

## Keine lokalen Pfade

Das Projekt darf keine privaten Rechnerpfade in Doku oder Code benoetigen.
Startpfade ergeben sich aus dem aktuellen Repository-Ordner, Docker-Mounts oder
Container-internen Arbeitsverzeichnissen.

## Keine Secrets im Code

Auth0-Client-ID und Domain sind fuer eine SPA oeffentliche Konfiguration. Alles,
was geheim ist, bleibt lokal:

- OpenWeatherMap API Key
- PostgreSQL Passwort
- pgAdmin Passwort
- ngrok Authtoken
- Social-Provider Secrets in Auth0 oder beim Provider

Das Frontend bekommt keinen OpenWeatherMap Key und kein Auth0 Client Secret.

