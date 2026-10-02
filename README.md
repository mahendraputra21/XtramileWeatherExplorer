# XtramileWeatherExplorer

Production-grade ASP.NET Core Web API + Blazor Web App (Interactive Server) per Xtramile Solutions Practical Test. Onion architecture (folders) + CQRS/MediatR + EF Core InMemory + OpenWeatherMap.

## Architecture

Onion via folders inside `src/Api`:
- `Domain` — Country, City, WeatherNote, Temperature.F->C. No dependencies.
- `Application` — MediatR queries/commands, DTOs, repository/service interfaces, ValidationBehavior. No EF Core.
- `Infrastructure` — AppDbContext (InMemory), repositories (.AsNoTracking), OpenWeatherMapService.
- `Api` — Thin controllers -> MediatR, ExceptionHandlingMiddleware (404/400/502/500).

See `AI-INSTRUCTIONS.md` for full layer/CQRS/naming rules.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | /api/countries | List countries |
| GET | /api/countries/{countryCode}/cities | Cities for country |
| GET | /api/weather/{cityName} | Current weather via IWeatherService |
| POST | /api/weather/notes | Create weather note (201) |

Weather payload includes: Location, TimeUtc, Wind (speed/direction), Visibility, Pressure, Sky, Temperature F/C, DewPoint F/C, Humidity.

Error codes: 404 only for not-found, 502 for upstream weather failure, 400 for validation.

## Setup

### Prerequisites
- .NET SDK 10.0.x

### Configure OpenWeatherMap

ApiKey is read only by the API via `IConfiguration["OpenWeatherMap:ApiKey"]` in `OpenWeatherMapService` — never exposed to the Blazor frontend.

Local development uses .NET User Secrets (no hardcoded keys, no committed secrets):

```bash
# One-time: UserSecretsId is already configured in src/Api/Api.csproj
# Set the key locally (stored outside the repo at %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json):
dotnet user-secrets --project src/Api/Api.csproj set "OpenWeatherMap:ApiKey" "YOUR_KEY"

# Verify (value is stored outside the repo):
dotnet user-secrets --project src/Api/Api.csproj list

# Remove if needed:
dotnet user-secrets --project src/Api/Api.csproj remove "OpenWeatherMap:ApiKey"
```

Production/deployment: provide the key via environment variable or configuration provider (no code change required):

```bash
# Environment variable (double underscore maps to colon):
OpenWeatherMap__ApiKey=YOUR_KEY
# or appsettings.Production.json / Azure Key Vault / etc. with key OpenWeatherMap:ApiKey
```

Do not paste the real key into `appsettings.json` or any committed file.

### Run API

```bash
dotnet run --project src/Api/Api.csproj
# -> http://localhost:5239 (see src/Api/Properties/launchSettings.json)
```

### Run Blazor Web App

Configure Api base URL in `src/Web/appsettings.json` (`Api:BaseUrl` default `http://localhost:5239/`):

```bash
dotnet run --project src/Web/Web.csproj
# -> http://localhost:5243
```

The Web App calls the API exclusively via HTTP (WeatherApiClient).

## Build

```bash
dotnet build
```

## Tests (offline, no live network)

```bash
dotnet test
# or verbose
dotnet test --verbosity normal
```

- `tests/Domain.Tests` — Temperature conversion (freezing, boiling, -40, body temp)
- `tests/Application.Tests` — Query/Command handlers (validation, persistence, temp conversion), mocked IWeatherService
- `tests/Api.IntegrationTests` — WebApplicationFactory with mocked IWeatherService and InMemory DB. Verifies 200/201/400/404/502 mapping.

## EF Core

Provider: `Microsoft.EntityFrameworkCore.InMemory` ("XtramileWeather"). All reads use `.AsNoTracking()`. Seed: 4 countries (AU, ID, US, JP) x 4 cities.

## Design Decisions

- Onion as folders per PDF ("use folder instead of project if AI agent").
- Application has no EF Core — repositories are interface-based.
- Temperature conversion lives in Domain ValueObject.
- Only `POST /api/weather/notes` implemented (PDF says notes OR favorites, we choose notes).
- `GET /api/weather/{cityName}` kept exactly as specified; city lookup via OWM query param.
- External failures -> 502, not 404.

## AI Prompt Configuration

See `AI-INSTRUCTIONS.md` for architecture, layer boundaries, CQRS, naming, EF, weather, and error-mapping rules used during implementation.
