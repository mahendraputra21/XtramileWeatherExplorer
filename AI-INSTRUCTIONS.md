# AI Instructions — XtramileWeatherExplorer

## Architecture — Onion (folders inside Api project)

Dependency direction: Domain -> Application -> Infrastructure -> Api (Presentation).

- **Domain** (`src/Api/Domain`): Entities (Country, City, WeatherNote) and ValueObject Temperature (F->C). Zero framework dependencies. No EF Core, no MediatR, no FluentValidation imports.
- **Application** (`src/Api/Application`): CQRS with MediatR (IMediator, IRequest/IRequestHandler), DTOs, repository interfaces (ICountryRepository, ICityRepository, IWeatherNoteRepository), IWeatherService, Behaviors (ValidationBehavior), FluentValidation validators. Must NOT reference Microsoft.EntityFrameworkCore. Must NOT contain DbContext or DbSet.
- **Infrastructure** (`src/Api/Infrastructure`): Implements Application interfaces. Persistence (AppDbContext, SeedData, Repository implementations using EF Core InMemory with .AsNoTracking() on all reads). External (OpenWeatherMapService : IWeatherService via HttpClient). References Domain and Application.
- **Api** (`src/Api/Api`): Thin controllers routing to MediatR, ExceptionHandlingMiddleware, Program.cs wiring. Depends on all inner layers via DI. Controllers must not contain business logic.

## CQRS Rules

- Queries: `*Query : IRequest<TResponse>` + `*Handler : IRequestHandler<*,*>`. Commands: `*Command : IRequest<TResponse>` + Handler + Validator.
- Read queries via repositories must use `.AsNoTracking()`.
- Validation via FluentValidation pipeline (`ValidationBehavior<TRequest,TResponse>`). Application tests assert ValidationException; HTTP 400 is asserted in API/integration layer.
- Handlers must not contain HTTP concerns (no StatusCodes, no IActionResult). Those live in controllers/middleware.

## Naming Conventions

- Queries: `GetCountriesQuery`, `GetCitiesByCountryQuery`, `GetWeatherQuery`
- Commands: `CreateWeatherNoteCommand`
- DTOs: `*Dto` (CountryDto, CityDto, WeatherDto, CreateWeatherNoteDto)
- Repositories: `I*Repository` / `*Repository`
- Exceptions: `NotFoundException` -> 404; `ExternalServiceException` -> 502 BadGateway (not 404); `ValidationException` -> 400
- Controllers: `*Controller` with `[Route("api/...")]`

## EF Core

- InMemory provider `Microsoft.EntityFrameworkCore.InMemory` named "XtramileWeather". No other DB.
- Seed Countries (AU, ID, US, JP) + 4 cities each in SeedData.cs.
- Repositories encapsulate all EF access; Application never touches DbContext.

## Weather Integration

- `IWeatherService.GetCurrentWeatherAsync(cityName, ct)` behind OpenWeatherMapService.
- Uses OpenWeatherMap Current Weather API 2.5 with `units=imperial` so temperature arrives as Fahrenheit; converts to Celsius in Domain/Application via Temperature.ToCelsius().
- ApiKey via `OpenWeatherMap:ApiKey` configuration (user-secrets). Tests mock IWeatherService — zero live calls.

## Error Mapping (required)

- City/weather not found (NotFoundException) -> 404
- External/OWM failures (ExternalServiceException) -> 502 Bad Gateway
- Validation failures -> 400
- Do not map every OWM failure to 404.

## Frontend

- Blazor Web App InteractiveServer. Must call backend exclusively via HTTP (WeatherApiClient HttpClient). Never bypass API with direct DbContext.

## Testing

- Domain.Tests: Temperature value object edge cases.
- Application.Tests: Handler unit tests with repository mocks / InMemory Db.
- Api.IntegrationTests: WebApplicationFactory with mocked IWeatherService.

## Prohibited

- No IAppDbContext with DbSet in Application.
- No favorites feature (only POST /api/weather/notes).
- No extra endpoints or value objects beyond what is required.
