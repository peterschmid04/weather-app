# Backend-Solution-Dateien

Dieser Ordner enthaelt die .NET Solution fuer Backend und Tests.

## weatherAPI.sln

Die Solution verbindet:

- `weatherAPI/weatherAPI.csproj`: ASP.NET Core Backend.
- `weatherAPI.Test/weatherAPI.Test.csproj`: Unit-Test-Projekt.

Die Solution ist der beste Einstieg fuer `dotnet build` und `dotnet test`.

## weatherAPI

Das eigentliche Backend-Projekt. Es stellt REST-Endpunkte bereit, prueft Auth0
JWTs, ruft OpenWeatherMap auf und speichert nutzerbezogene Daten in PostgreSQL.

Weitere Details stehen in:

- `weatherAPI/API_FILES.md`

## weatherAPI.Test

Unit-Tests fuer Services und Mapping-Logik. Die Tests mocken den externen
OpenWeatherMap-Client, damit sie ohne Internet und ohne echte API-Keys laufen.

Weitere Details stehen in:

- `weatherAPI.Test/TEST_FILES.md`

## Build und Test

Im Docker-Backend-Container:

```sh
dotnet build /workspace/weatherAPIOAuth/weatherAPI.sln --no-restore
dotnet test /workspace/weatherAPIOAuth/weatherAPI.sln --no-build --no-restore
```

## Architekturidee

Das Backend folgt aktuell einem Minimal-API-Stil. Die Datei `Program.cs`
registriert Services, Auth, Datenbank, Rate Limiting und Endpunkte. Die
Fachlogik liegt zunehmend in Services und Modellen, waehrend EF Core das ORM zur
Datenbank ist.

Bei spaeterem Wachstum koennen die Endpunkte aus `Program.cs` in eigene
Feature-Dateien ausgelagert werden. Die vorhandene Dokumentation markiert schon,
welche Endpunktgruppen zusammengehoeren.

