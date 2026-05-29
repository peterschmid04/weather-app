# Frontend-Dateien

Dieses Verzeichnis enthaelt die React-Oberflaeche. Das Frontend ist Docker-first
gedacht und nutzt relative API-Pfade. Es braucht deshalb keine lokale Backend-URL
in einer eigenen Frontend-`.env`.

## Root des Frontends

`README.md`

Kurze Frontend-Dokumentation. Die vollstaendige Projektanleitung bleibt im
Root-`README.md`.

`FRONTEND_FILES.md`

Diese Datei. Sie erklaert die Frontend-Dateien und deren Aufgaben.

`package.json`

Definiert npm-Scripts und Abhaengigkeiten. Wichtige Scripts:

- `npm start`: startet den React-Dev-Server im Container.
- `npm run build`: baut die statischen Frontend-Dateien zur Pruefung.
- `npm test`: startet das CRA-Testsystem.

`package-lock.json`

Fixiert npm-Abhaengigkeiten fuer reproduzierbare Installationen mit `npm ci`.
Diese Datei wird nicht manuell editiert.

`jsconfig.json`

Setzt `src` als Basis fuer Frontend-Imports.

## public

`public/index.html`

HTML-Vorlage fuer die React-App. Der React-Build ersetzt zur Laufzeit Platzhalter
und haengt die gebauten JavaScript-/CSS-Dateien ein.

`public/manifest.json`

Metadaten fuer installierbare Browser-Apps, z. B. Name, Icons und Start-URL.
JSON erlaubt keine Kommentare, deshalb ist die Aufgabe hier dokumentiert.

`public/robots.txt`

Crawler-Hinweis fuer die lokale App.

`public/icon.svg`, `favicon.ico`, `logo192.png`, `logo512.png`

App-Icons fuer Browser-Tab, Manifest und mobile Installationen.

## src Einstieg

`src/index.js`

Startpunkt der React-App. Hier wird der Auth0Provider eingerichtet. Die Werte
kommen aus Docker Compose als `REACT_APP_AUTH0_*` Variablen.

`src/App.js`

Zentraler Container fuer:

- Auth0 Login-Zustand.
- Token-Abruf fuer Backend-Requests.
- Wetterdatenabruf.
- Laden des Standardfavoriten.
- Laden und Speichern des Themes.
- Fehlerstatus und Suchstatus.
- Zusammensetzen von Sidebar, Forecast, Highlights und Nutzerdatenbereich.

`src/App.css`

Globale Layout- und Theme-Regeln fuer die Hauptoberflaeche.

## src/components

`LoginOptions.js` und `LoginOptions.css`

Login-Oberflaeche fuer Auth0. Bietet Login, Registrierung und Social Login ueber
konfigurierte Auth0 Connections an.

`Sidebar.js` und `Sidebar.css`

Linke Seitenleiste. Aufgaben:

- Suchfeld.
- Suchvorschlaege.
- Kurzverlauf.
- Schnellfavoriten.
- Wettericon, Temperatur, Datum, Uhrzeit und Beschreibung.

`Forecast.js` und `Forecast.css`

Zeigt die Forecast-Karten. Die Daten kommen bereits vom Backend als passende
Tagesobjekte.

`Highlights.js` und `Highlights.css`

Zeigt sechs Highlight-Kacheln:

- UV-Index.
- Wind.
- Sonnenaufgang und Sonnenuntergang.
- Luftfeuchtigkeit.
- Sichtweite.
- Luftqualitaet.

`UVIndex.js` und `UVIndex.css`

Spezielle Halbkreis-Anzeige fuer den UV-Wert.

`UserDataPanel.js` und `UserDataPanel.css`

Bereich fuer nutzerbezogene Daten:

- Favoriten.
- Standardfavorit.
- Theme-Auswahl.
- Eingehende Freigaben.
- Angenommene Freigaben.
- Ausgehende Freigaben.

`Stations.js` und `Stations.css`

Wetterstationsverwaltung:

- Eigene Stationen anlegen, bearbeiten und loeschen.
- Geteilte Stationen lesen oder mit Messwerten befuellen.
- Messwerte eintragen.
- Messwertliste anzeigen.

## src/utils

`apiUtils.js`

Zentrale API-Pfadfunktion. Im Docker-Betrieb bleiben Backend-Aufrufe relativ,
damit localhost, Container-Netzwerk und ngrok sauber funktionieren.

`weatherUtils.js`

Ordnet OpenWeatherMap-Wettercodes lokalen SVG-Icons zu.

`statusUtils.js`

Wandelt Zahlenwerte wie Wind, Luftfeuchtigkeit und Sichtweite in deutsche
Statuslabels um.

`localizationUtils.js`

Enthaelt deutsche Anzeige- und Suchhelfer:

- Tagesnamen.
- Wetterbeschreibungen.
- Stadtanzeige.
- Suchvorschlaege.
- Normalisierung fuer Umlaute und Sonderzeichen.

## src/images

Lokale SVG-Dateien fuer Wetterzustaende. Sie werden importiert und durch den
React-Build optimiert gebuendelt.

## Datenfluss im Frontend

1. Nutzer meldet sich ueber Auth0 an.
2. `App.js` holt ein Access Token.
3. `authFetchJson` haengt `Authorization: Bearer ...` an API-Requests.
4. Backend antwortet mit Wetterdaten oder nutzerbezogenen Daten.
5. React-State aktualisiert die sichtbaren Komponenten.

