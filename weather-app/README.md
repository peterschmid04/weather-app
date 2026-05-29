# Frontend-Dokumentation

Dieses Verzeichnis enthält die React-Oberfläche der Weather App. Das Frontend wird im Projekt über Docker Compose gestartet und spricht die Backend-API über relative Pfade wie `/weather`, `/favorites` oder `/stations` an. Dadurch funktioniert die App lokal und über den optionalen ngrok-Tunnel ohne hart codierte lokale Backend-URL.

## Einstieg

- `src/index.js` startet React und rendert die App in `public/index.html`.
- `src/App.js` hält den zentralen Dashboard-State, Auth0-Tokenabruf, Wetterabruf, Theme-Laden und die Übergabe an die Hauptkomponenten.
- `src/components/` enthält die sichtbaren UI-Bausteine.
- `src/utils/` enthält reine Hilfsfunktionen für API-Pfade, Wettericons, Statuslabels und deutsche Anzeige-/Suchtexte.
- `src/images/` enthält lokale SVG-Assets für Wetterzustände und Highlight-Icons.

## Komponenten

- `LoginOptions.js`: Login-, Registrieren- und Social-Login-Schaltflächen über Auth0.
- `Sidebar.js`: Suchfeld, Suchvorschläge, Kurzverlauf, Favoriten-Schnellzugriff und aktuelles Wetter links.
- `Forecast.js`: Tageskarten für die vom Backend vorbereitete Vorhersage.
- `Highlights.js`: UV-Index, Wind, Sonnenzeiten, Luftfeuchte, Sichtweite und Luftqualität.
- `UVIndex.js`: Halbkreis-Anzeige für den UV-Wert.
- `UserDataPanel.js`: Favoriten, Theme-Auswahl und Wetterstationsfreigaben.
- `Stations.js`: Eigene und geteilte Wetterstationen, Messwertformular und Messwertliste.

## Konfiguration

Das Frontend bekommt nur öffentliche SPA-Konfiguration aus Docker Compose:

- Auth0 Domain
- Auth0 Client ID
- Auth0 Audience
- Auth0 Scope
- Auth0 Connection-Namen

Der OpenWeatherMap-Key bleibt ausschließlich im Backend. Auth0 Client Secrets und Social-Provider-Secrets gehören ebenfalls nicht ins Frontend.

## Start

Normalerweise wird das Frontend nicht separat gestartet, sondern über den Projekt-Root:

```sh
docker compose up -d
```

Für den Build-Check im Container:

```sh
docker compose exec -T frontend sh -c "BUILD_PATH=/tmp/weather-app-build npm run build"
```
