# Weather App To-do

## Freigabe und Rollen

- [x] Freigabe/Rollen als offenes Konzept dokumentieren.
- [x] Wetterstationen koennen an andere Nutzer freigegeben werden.
- [x] Freigabe-Workflow planen: Nutzer A gibt Station an Nutzer B frei.
- [x] Freigabe-Workflow planen: Nutzer B darf Messwerte eintragen, aber Station nicht besitzen.
- [x] Freigabe-Workflow planen: Vertretung fuer Messwerterfassung, nicht nur Urlaubsvertretung.
- [x] Datenmodell fuer Freigaben planen, z. B. `WeatherStationShares`.
- [x] Auth0-User-Suche oder Einladungskonzept fuer Freigaben planen.
- [x] Auth0 Rollen/Permissions dokumentieren: `region:all`, `region:eu`, Standardzugriff Deutschland.
- [x] Auth0 Rollen in der Praesentation erklaeren: Alle, Europa, Deutschland.
- [x] Codebezug dokumentieren: `RegionAuthorization` nutzt Auth0 Permissions.
- [x] Teilen/Freigeben von Wetterstationen mit Auth0-Nutzern konzeptionell beschreiben.

## Start und Deployment testen

- [x] Nach den neuen Features kompletten frischen Start testen: `docker compose down`, dann `docker compose up -d`.
- [x] Nach den neuen Features testen, dass Frontend, Backend, DB und pgAdmin gemeinsam starten.
- [ ] Nach neuen Features pruefen: frischer Clone plus `.env` plus `docker compose up -d` reicht.

## Fachliche Funktionen testen

- [ ] Wetterdashboard zeigt aktuelle Wetterdaten.
- [ ] Stadtsuche funktioniert.
- [ ] Forecast funktioniert.
- [ ] UV-Index funktioniert.
- [ ] Luftqualitaet funktioniert.
- [ ] Authentifizierter Zugriff funktioniert.
- [ ] Suchhistorie funktioniert.
- [ ] Favoriten funktionieren.
- [ ] Eigene Wetterstationen funktionieren.
- [ ] GUI-Themes funktionieren.

## Frontend und Navigation

- [x] Navigation/Routing pruefen: kein separates Routing noetig, im README begruendet.
- [x] Box neben den Favoriten zeigt die Freigabe-Verwaltung fuer Wetterstationen statt Suchverlauf.
- [ ] Responsive Darstellung mit Stationen/Favoriten nach dem Umbau visuell pruefen.

## Extra-Feature-Ideen

- [x] Extra Feature Idee dokumentiert: Wetterstation an andere Nutzer teilen.
- [x] Extra Feature Idee dokumentiert: Vertretung fuer Wetterstationsmesswerte.
- [x] Extra Feature Idee dokumentiert: Rollenbasierte Admin-/Region-Ansicht.
- [x] Extra Feature umsetzen: Wetterstation an andere Nutzer teilen.
- [x] Extra Feature umsetzen: Vertretung fuer Wetterstationsmesswerte.
- [ ] Extra Feature umsetzen: Rollenbasierte Admin-/Region-Ansicht.
