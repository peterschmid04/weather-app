# Architekturdiagramm

Dieses Diagramm zeigt die Laufzeitarchitektur der Weather App. Es nutzt Mermaid
`flowchart`-Syntax, weil diese in Markdown stabil und breit unterstuetzt ist.

Mermaid-Regeln, die hier bewusst eingehalten werden:

- Das Diagramm beginnt mit dem Diagrammtyp `flowchart`.
- `LR` setzt die Leserichtung von links nach rechts.
- Zusammengehoerige Teile werden mit `subgraph` gruppiert.
- Labels mit Leerzeichen, Slashs oder Sonderzeichen stehen in Anfuehrungszeichen.
- Kommentare stehen auf eigenen Zeilen und beginnen mit `%%`.
- Der Knotenname `end` wird nicht verwendet, weil Mermaid das als Blockende lesen kann.

```mermaid
flowchart LR
  %% Nutzer und externe Dienste
  User["Nutzer<br/>Browser"]
  Auth0["Auth0<br/>Universal Login<br/>JWT / OAuth"]
  OWM["OpenWeatherMap API<br/>Weather / Forecast<br/>Air Quality / UV"]
  Ngrok["ngrok optional<br/>Public HTTPS URL"]

  %% Docker Compose Runtime
  subgraph Compose["Docker Compose Stack"]
    Frontend["frontend<br/>React SPA<br/>Port 3000"]
    Backend["backend<br/>ASP.NET Core Minimal API<br/>Port 5122"]
    PgAdmin["pgAdmin<br/>DB-GUI<br/>Port 5050"]
    Database["db<br/>PostgreSQL<br/>weather_app"]
  end

  %% Backend-Aufteilung
  subgraph ApiGroups["Backend Endpunktgruppen"]
    WeatherApi["Wetter<br/>/weather /forecast<br/>/uv /airquality"]
    UserApi["Nutzer-Daten<br/>/history /favorites<br/>/theme /my-profile"]
    StationApi["Wetterstationen<br/>/stations<br/>/station-shares"]
    AccessApi["Zugriff<br/>/access<br/>RegionAuthorization"]
  end

  %% Persistente Daten
  subgraph DataModel["PostgreSQL Tabellen"]
    Profiles["UserProfiles"]
    Cities["Cities"]
    History["SearchHistory"]
    Favorites["FavoriteCities"]
    Themes["UserThemePreferences"]
    Stations["WeatherStations"]
    Measurements["WeatherStationMeasurements"]
    Shares["WeatherStationShares"]
    RequestLogs["WeatherRequestLogs"]
  end

  %% Konfiguration und Volumes
  Env["lokale .env<br/>Auth0 / OWM / DB<br/>ngrok optional"]
  Volumes["Docker Volumes<br/>PostgreSQL / pgAdmin<br/>NuGet / node_modules"]
  Logs["logs optional<br/>nur bei LOGS=true"]

  %% Browser-Fluss
  User -->|"oeffnet localhost:3000<br/>oder ngrok URL"| Frontend
  Ngrok -->|"Tunnel zu frontend:3000"| Frontend
  Frontend -->|"Login / Registrierung"| Auth0
  Auth0 -->|"Access Token"| Frontend
  Frontend -->|"relative API Requests<br/>Authorization: Bearer"| Backend

  %% Backend-Fluss
  Backend -->|"validiert JWT<br/>Issuer / Audience / Claims"| Auth0
  Backend -->|"HTTP Client<br/>API Key serverseitig"| OWM
  Backend -->|"EF Core / Npgsql"| Database
  Backend --> WeatherApi
  Backend --> UserApi
  Backend --> StationApi
  Backend --> AccessApi

  %% Datenbank-Fluss
  PgAdmin -->|"bearbeiten / ansehen"| Database
  Database --> Profiles
  Database --> Cities
  Database --> History
  Database --> Favorites
  Database --> Themes
  Database --> Stations
  Database --> Measurements
  Database --> Shares
  Database --> RequestLogs

  %% Konfiguration
  Env -.-> Frontend
  Env -.-> Backend
  Env -.-> Database
  Env -.-> PgAdmin
  Env -.-> Ngrok
  Volumes -.-> Frontend
  Volumes -.-> Backend
  Volumes -.-> Database
  Volumes -.-> PgAdmin
  Logs -.-> Backend

  classDef user fill:#fff7cc,stroke:#8a6d00,color:#111;
  classDef external fill:#edf5ff,stroke:#2f6fad,color:#111;
  classDef service fill:#eef9f0,stroke:#2f7d46,color:#111;
  classDef api fill:#f3edff,stroke:#6c4fb3,color:#111;
  classDef data fill:#fff0f0,stroke:#a94442,color:#111;
  classDef config fill:#f2f2f2,stroke:#666,color:#111;

  class User user;
  class Auth0,OWM,Ngrok external;
  class Frontend,Backend,PgAdmin service;
  class WeatherApi,UserApi,StationApi,AccessApi api;
  class Database,Profiles,Cities,History,Favorites,Themes,Stations,Measurements,Shares,RequestLogs data;
  class Env,Volumes,Logs config;
```

## Lesart

1. Der Nutzer oeffnet das React-Frontend lokal oder ueber den optionalen
   ngrok-Tunnel.
2. Das Frontend leitet Login und Registrierung an Auth0 weiter.
3. Auth0 gibt ein Access Token zurueck.
4. Das Frontend ruft das Backend ueber relative API-Pfade auf und sendet das
   Token als Bearer Token mit.
5. Das Backend validiert das Token, prueft Region-Permissions und fuehrt
   fachliche REST-Endpunkte aus.
6. Wetterdaten kommen serverseitig von OpenWeatherMap. Der API-Key liegt nicht
   im Frontend.
7. Favoriten, Verlauf, Themes, Wetterstationen, Messwerte und Freigaben werden
   per EF Core in PostgreSQL gespeichert.
8. pgAdmin verbindet sich innerhalb des Docker-Netzwerks mit PostgreSQL.
9. Docker-Volumes halten Daten und Entwicklungsabhaengigkeiten ausserhalb des
   Git-Repositories.

## Mermaid-Quellen

- Mermaid Architecture Diagrams: https://mermaid.js.org/syntax/architecture.html
- Mermaid Flowchart Syntax: https://mermaid.js.org/syntax/flowchart.html
- Mermaid Syntax Reference: https://mermaid.js.org/intro/syntax-reference.html
