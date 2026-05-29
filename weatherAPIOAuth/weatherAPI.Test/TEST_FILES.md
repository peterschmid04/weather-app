# Test-Dateien

Dieses Projekt enthaelt Unit-Tests fuer das Backend. Es ist bewusst klein und
testet aktuell die Service-Schicht ohne echte externe API-Aufrufe.

## weatherAPI.Test.csproj

Testprojekt fuer .NET. Wichtige Pakete:

- MSTest fuer Testausfuehrung.
- FluentAssertions fuer lesbare Assertions.
- Moq fuer gemockte Interfaces.

Das Projekt referenziert `weatherAPI.csproj`, damit die echten Services und
Modelle getestet werden.

## WeatherApiServicesTests.cs

Unit-Testklasse fuer:

- `AirQualityService`
- `UvService`
- `ForecastService`
- `WeatherService`

Der externe `IOpenWeatherApiService` wird gemockt. Dadurch:

- wird kein OpenWeatherMap-Key benoetigt,
- gibt es keine Internetabhaengigkeit,
- bleiben die erwarteten Werte stabil,
- laufen die Tests schnell im Container.

## Teststrategie

Aktueller Fokus:

- Service ruft den richtigen API-Client auf.
- Service mappt externe Daten in interne DTOs.
- Fehler-/Nullfaelle werden kontrolliert behandelt.

Sinnvolle naechste Tests:

- RegionAuthorization.
- Favoriten-Duplikate.
- Suchverlauf-Trim auf drei Eintraege.
- Wetterstationsfreigaben.
- Endpoint-Integrationstests mit WebApplicationFactory.

