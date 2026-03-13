using System.Globalization;
using System.Text.Json;
using Deploy.Interfaces;

namespace Deploy.Services;

public class AirQualityService : IAirQualityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAirQualityRepository _repository;
    private readonly string _apiBaseUrl;

    public AirQualityService(
        IHttpClientFactory httpClientFactory,
        IAirQualityRepository repository,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _repository = repository;
        _apiBaseUrl = configuration["AirQuality:ApiBaseUrl"] ?? configuration["AirQuality:ApiUrl"]
            ?? throw new InvalidOperationException("AirQuality:ApiUrl is not configured.");
    }

    public async Task<string> GetAirQualityJsonAsync(double latitude, double longitude, string? hourly)
    {
        var hourlyNormalized = string.IsNullOrWhiteSpace(hourly) ? "pm10,pm2_5" : hourly.Trim();
        var requestUrl = BuildRequestUrl(latitude, longitude, hourlyNormalized);

        // 1) Try external API
        var fetched = await TryFetchAsync(requestUrl);
        if (fetched is not null)
        {
            // Store best-effort (but still return fetched even if DB insert fails)
            try
            {
                await _repository.EnsureTableExistsAsync();
                await _repository.InsertReadingAsync(latitude, longitude, hourlyNormalized, fetched);
            }
            catch
            {
                // Intentionally swallow: user asked to avoid empty JSON;
                // external data is still useful even if DB is temporarily unavailable.
            }

            return fetched;
        }

        // 2) Fallback to DB
        await _repository.EnsureTableExistsAsync();
        var latest = await _repository.GetLatestReadingAsync(latitude, longitude, hourlyNormalized);
        if (!string.IsNullOrWhiteSpace(latest))
        {
            return latest;
        }

        // 3) Fallback to mock JSON (same basic shape as Open-Meteo response)
        return BuildMockJson(latitude, longitude, hourlyNormalized);
    }

    private async Task<string?> TryFetchAsync(string requestUrl)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            // Validate JSON so we don't store garbage / HTML error pages.
            using var _ = JsonDocument.Parse(content);
            return content;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildMockJson(double latitude, double longitude, string hourly)
    {
        var now = DateTime.UtcNow;
        var hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        var hourIso = hour.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);

        var hourlyFields = hourly
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .ToArray();

        var hourlyUnits = new Dictionary<string, string>
        {
            ["time"] = "iso8601"
        };

        if (hourlyFields.Contains("pm10"))
        {
            hourlyUnits["pm10"] = "μg/m³";
        }

        if (hourlyFields.Contains("pm2_5"))
        {
            hourlyUnits["pm2_5"] = "μg/m³";
        }

        var hourlyPayload = new Dictionary<string, object?>
        {
            ["time"] = new[] { hourIso }
        };

        if (hourlyFields.Contains("pm10"))
        {
            hourlyPayload["pm10"] = new double?[] { null };
        }

        if (hourlyFields.Contains("pm2_5"))
        {
            hourlyPayload["pm2_5"] = new double?[] { null };
        }

        var payload = new
        {
            latitude,
            longitude,
            generationtime_ms = 0.0,
            utc_offset_seconds = 0,
            timezone = "GMT",
            timezone_abbreviation = "GMT",
            elevation = 38.0,
            hourly_units = hourlyUnits,
            hourly = hourlyPayload
        };

        return JsonSerializer.Serialize(payload);
    }

    private string BuildRequestUrl(double latitude, double longitude, string hourly)
    {
        var separator = _apiBaseUrl.Contains('?') ? "&" : "?";
        var baseUrl = _apiBaseUrl.TrimEnd('&', '?');
        var encodedHourly = Uri.EscapeDataString(hourly);

        return $"{baseUrl}{separator}latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&hourly={encodedHourly}";
    }
}

