# Docker-Dateien

Dieser Ordner enthaelt nur Dateien, die Docker Compose beim lokalen Start
direkt einbindet. Die eigentliche Service-Definition liegt im Root in
`docker-compose.yml`.

## docker-compose.yml im Root

Der Compose-Stack ist fuer lokale Entwicklung gebaut. Er startet:

- PostgreSQL als relationale Datenbank.
- pgAdmin als Browser-GUI fuer PostgreSQL.
- Das ASP.NET Core Backend im .NET SDK Container.
- Das React Frontend im Node Container.
- Optional ngrok, wenn das Profil und die passenden `.env` Werte gesetzt sind.

Die Container verwenden Docker-Volumes fuer Daten und Abhaengigkeiten. Dadurch
bleiben Datenbankdaten, NuGet-Pakete und `node_modules` zwischen Starts erhalten,
ohne dass diese Ordner ins Repository geschrieben werden.

## docker/postgres/init/001-ef-migrations-history.sql

Diese SQL-Datei wird nur beim ersten Initialisieren eines neuen PostgreSQL
Volumes ausgefuehrt. Sie legt die EF-Core-Migrationstabelle an, falls sie noch
nicht existiert.

Warum diese Datei existiert:

- EF Core fragt `__EFMigrationsHistory` beim Migrationsstart ab.
- Auf einem komplett frischen Volume kann die Tabelle noch fehlen.
- Die Datei verhindert relation-errors beim ersten Docker-Start.

Die echte Migration der Tabellen bleibt weiterhin Aufgabe von EF Core im
Backend.

## docker/pgadmin/servers.json

Diese JSON-Datei registriert den Datenbankserver automatisch in pgAdmin. Dadurch
muss man den Host `db` nicht jedes Mal manuell eintragen.

Wichtige Werte:

- `Host`: `db`, weil pgAdmin und PostgreSQL im gleichen Docker-Netzwerk laufen.
- `Port`: `5432`.
- `MaintenanceDB`: Standarddatenbank fuer die Verbindung.

Passwoerter stehen nicht in dieser Datei. pgAdmin bekommt sein Login ueber
`.env`; die Datenbankverbindung nutzt die in Compose gesetzten PostgreSQL-Werte.

## ngrok

ngrok ist optional. Wenn es aktiv ist, veroeffentlicht es das Frontend auf Port
`3000`. Das Backend wird nicht separat nach aussen geoeffnet, sondern bleibt
ueber den Frontend-Proxy erreichbar.

Die ngrok-Werte gehoeren in `.env`:

- `NGROK_AUTHTOKEN`
- `NGROK_URL`

Sind diese Werte leer, soll die App weiterhin normal lokal ueber Docker Compose
laufen.

## Was hier nicht liegt

In diesem Ordner liegen bewusst keine tiefen Unterordner fuer Backend- oder
Frontend-Builds. Build-Artefakte gehoeren in Docker-Volumes oder Container, nicht
in Git.

