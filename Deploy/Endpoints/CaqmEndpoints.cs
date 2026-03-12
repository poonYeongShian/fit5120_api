using Deploy.Interfaces;

namespace Deploy.Endpoints;

public static class CaqmEndpoints
{
    public static void MapCaqmEndpoints(this WebApplication app)
    {
        app.MapGet("/api/caqm/current", HandleGetCurrentAsync)
            .WithName("GetCurrentCaqmReading")
            .WithOpenApi();

        app.MapGet("/api/caqm/latest", HandleGetLatestAsync)
            .WithName("GetLatestStoredCaqmReading")
            .WithOpenApi();
    }

    private static async Task<IResult> HandleGetCurrentAsync(ICaqmService caqmService)
    {
        try
        {
            var content = await caqmService.FetchAndStoreCurrentReadingAsync();
            return Results.Content(content, "application/json");
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem($"Error calling external API: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error saving data to PostgreSQL: {ex.Message}");
        }
    }

    private static async Task<IResult> HandleGetLatestAsync(ICaqmService caqmService)
    {
        try
        {
            var json = await caqmService.GetLatestStoredReadingAsync();

            if (json is null)
            {
                return Results.NotFound("No readings stored yet.");
            }

            return Results.Content(json, "application/json");
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error reading data from PostgreSQL: {ex.Message}");
        }
    }
}
