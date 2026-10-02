using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext db)
    {
        if (await db.Countries.AnyAsync()) return;

        var countries = new[]
        {
            new Country { Code = "AU", Name = "Australia" },
            new Country { Code = "ID", Name = "Indonesia" },
            new Country { Code = "US", Name = "United States" },
            new Country { Code = "JP", Name = "Japan" },
        };
        db.Countries.AddRange(countries);

        var cities = new[]
        {
            new City { Id = 1, Name = "Sydney", CountryCode = "AU" },
            new City { Id = 2, Name = "Melbourne", CountryCode = "AU" },
            new City { Id = 3, Name = "Brisbane", CountryCode = "AU" },
            new City { Id = 4, Name = "Perth", CountryCode = "AU" },

            new City { Id = 5, Name = "Jakarta", CountryCode = "ID" },
            new City { Id = 6, Name = "Surabaya", CountryCode = "ID" },
            new City { Id = 7, Name = "Bali", CountryCode = "ID" },
            new City { Id = 8, Name = "Bandung", CountryCode = "ID" },

            new City { Id = 9, Name = "New York", CountryCode = "US" },
            new City { Id = 10, Name = "Los Angeles", CountryCode = "US" },
            new City { Id = 11, Name = "Chicago", CountryCode = "US" },
            new City { Id = 12, Name = "Seattle", CountryCode = "US" },

            new City { Id = 13, Name = "Tokyo", CountryCode = "JP" },
            new City { Id = 14, Name = "Osaka", CountryCode = "JP" },
            new City { Id = 15, Name = "Kyoto", CountryCode = "JP" },
            new City { Id = 16, Name = "Sapporo", CountryCode = "JP" },
        };
        db.Cities.AddRange(cities);
        await db.SaveChangesAsync();
    }
}
