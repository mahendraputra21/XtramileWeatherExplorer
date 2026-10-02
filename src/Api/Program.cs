using Api.Application.Common.Behaviors;
using Api.Application.Common.Interfaces;
using Api.Middleware;
using Api.Infrastructure.External;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// MediatR — scan Api assembly (contains Application/*)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation + pipeline
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("XtramileWeather"));

// Repositories (Application interface -> Infrastructure impl)
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IWeatherNoteRepository, WeatherNoteRepository>();

// IWeatherService -> OpenWeatherMap (typed HttpClient)
builder.Services.AddHttpClient<IWeatherService, OpenWeatherMapService>();

// CORS for Blazor Web App
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.EnsureSeededAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.MapControllers();

app.Run();

// For WebApplicationFactory
public partial class Program { }
