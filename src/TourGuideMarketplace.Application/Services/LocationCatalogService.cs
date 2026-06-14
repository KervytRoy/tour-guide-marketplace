using System.Globalization;
using System.Text;
using TourGuideMarketplace.Application.Interfaces;
using TourGuideMarketplace.Contracts.Locations;

namespace TourGuideMarketplace.Application.Services;

public sealed class LocationCatalogService : ILocationCatalogService
{
    private static readonly IReadOnlyList<LocationCatalogItem> Locations =
    [
        new("Lima", "Peru", "PE", "Lima", -12.046374m, -77.042793m),
        new("Cusco", "Peru", "PE", "Cusco", -13.531950m, -71.967463m),
        new("Arequipa", "Peru", "PE", "Arequipa", -16.409047m, -71.537451m),
        new("Trujillo", "Peru", "PE", "La Libertad", -8.111600m, -79.028700m),
        new("Ica", "Peru", "PE", "Ica", -14.067800m, -75.728600m),
        new("Puno", "Peru", "PE", "Puno", -15.840200m, -70.021900m),
        new("Huaraz", "Peru", "PE", "Ancash", -9.527800m, -77.527800m),
        new("Paracas", "Peru", "PE", "Ica", -13.835000m, -76.250000m),
        new("Mancora", "Peru", "PE", "Piura", -4.107800m, -81.047500m),
        new("Bogota", "Colombia", "CO", "Bogota", 4.711000m, -74.072100m),
        new("Cartagena", "Colombia", "CO", "Bolivar", 10.391000m, -75.479400m),
        new("Medellin", "Colombia", "CO", "Antioquia", 6.244200m, -75.581200m),
        new("Santa Marta", "Colombia", "CO", "Magdalena", 11.240800m, -74.199000m),
        new("Quito", "Ecuador", "EC", "Pichincha", -0.180700m, -78.467800m),
        new("Cuenca", "Ecuador", "EC", "Azuay", -2.900100m, -79.005900m),
        new("Guayaquil", "Ecuador", "EC", "Guayas", -2.189400m, -79.889100m),
        new("Mexico City", "Mexico", "MX", "Ciudad de Mexico", 19.432600m, -99.133200m),
        new("Oaxaca", "Mexico", "MX", "Oaxaca", 17.073200m, -96.726600m),
        new("Cancun", "Mexico", "MX", "Quintana Roo", 21.161900m, -86.851500m),
        new("Playa del Carmen", "Mexico", "MX", "Quintana Roo", 20.629600m, -87.073900m),
        new("Tulum", "Mexico", "MX", "Quintana Roo", 20.211400m, -87.465400m),
        new("Buenos Aires", "Argentina", "AR", "Buenos Aires", -34.603700m, -58.381600m),
        new("Bariloche", "Argentina", "AR", "Rio Negro", -41.133500m, -71.310300m),
        new("Mendoza", "Argentina", "AR", "Mendoza", -32.889500m, -68.845800m),
        new("Santiago", "Chile", "CL", "Region Metropolitana", -33.448900m, -70.669300m),
        new("Valparaiso", "Chile", "CL", "Valparaiso", -33.047200m, -71.612700m),
        new("San Pedro de Atacama", "Chile", "CL", "Antofagasta", -22.908700m, -68.199700m),
        new("La Paz", "Bolivia", "BO", "La Paz", -16.489700m, -68.119300m),
        new("Uyuni", "Bolivia", "BO", "Potosi", -20.460300m, -66.826100m),
        new("Montevideo", "Uruguay", "UY", "Montevideo", -34.901100m, -56.164500m),
        new("Rio de Janeiro", "Brazil", "BR", "Rio de Janeiro", -22.906800m, -43.172900m),
        new("Sao Paulo", "Brazil", "BR", "Sao Paulo", -23.555800m, -46.639600m),
        new("Antigua Guatemala", "Guatemala", "GT", "Sacatepequez", 14.558600m, -90.729500m),
        new("Panama City", "Panama", "PA", "Panama", 8.982400m, -79.519900m),
        new("San Jose", "Costa Rica", "CR", "San Jose", 9.928100m, -84.090700m),
        new("Havana", "Cuba", "CU", "La Habana", 23.113600m, -82.366600m),
        new("Santo Domingo", "Dominican Republic", "DO", "Distrito Nacional", 18.486100m, -69.931200m),
        new("Punta Cana", "Dominican Republic", "DO", "La Altagracia", 18.560100m, -68.372500m),
        new("San Juan", "Puerto Rico", "PR", "San Juan", 18.465500m, -66.105700m)
    ];

    public Task<IReadOnlyCollection<LocationSuggestionResponse>> SuggestAsync(
        string? query,
        int limit,
        CancellationToken cancellationToken)
    {
        limit = Math.Clamp(limit, 1, 20);

        var normalizedQuery = Normalize(query);
        var suggestions = string.IsNullOrWhiteSpace(normalizedQuery)
            ? Locations
                .OrderBy(location => location.Country)
                .ThenBy(location => location.City)
                .Take(limit)
            : Locations
                .Select(location => new
                {
                    Location = location,
                    Score = GetScore(location, normalizedQuery)
                })
                .Where(match => match.Score < int.MaxValue)
                .OrderBy(match => match.Score)
                .ThenBy(match => match.Location.Country)
                .ThenBy(match => match.Location.City)
                .Take(limit)
                .Select(match => match.Location);

        return Task.FromResult<IReadOnlyCollection<LocationSuggestionResponse>>(
            suggestions.Select(ToResponse).ToArray());
    }

    private static int GetScore(LocationCatalogItem location, string normalizedQuery)
    {
        var city = Normalize(location.City);
        var country = Normalize(location.Country);
        var region = Normalize(location.Region);
        var displayName = Normalize($"{location.City}, {location.Country}");

        if (city.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 0;
        }

        if (displayName.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 1;
        }

        if (country.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 2;
        }

        if (city.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 3;
        }

        if (region.Contains(normalizedQuery, StringComparison.Ordinal)
            || country.Contains(normalizedQuery, StringComparison.Ordinal)
            || displayName.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 4;
        }

        return int.MaxValue;
    }

    private static LocationSuggestionResponse ToResponse(LocationCatalogItem location)
    {
        return new LocationSuggestionResponse(
            location.City,
            location.Country,
            location.CountryCode,
            location.Region,
            location.Latitude,
            location.Longitude);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record LocationCatalogItem(
        string City,
        string Country,
        string CountryCode,
        string? Region,
        decimal? Latitude,
        decimal? Longitude);
}
