using Deploy.Interfaces;

namespace Deploy.Endpoints;

public static class AirQualityEndpoints
{
    public static void MapAirQualityEndpoints(this WebApplication app)
    {
        app.MapGet("/api/air-quality", HandleGetAirQualityAsync)
            .WithName("GetAirQuality")
            .WithOpenApi();
    }

    private static async Task<IResult> HandleGetAirQualityAsync(
        IAirQualityService airQualityService,
        double latitude = 52.52,
        double longitude = 13.41,
        string? hourly = "pm10,pm2_5")
    {
        try
        {
            var json = await airQualityService.GetAirQualityJsonAsync(latitude, longitude, hourly);
            return Results.Content(json, "application/json");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Unhandled error: {ex.Message}");
        }
    }
}

