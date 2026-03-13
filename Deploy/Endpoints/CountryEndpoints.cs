using Deploy.Interfaces;
using Deploy.Mappings;

namespace Deploy.Endpoints;

public static class CountryEndpoints
{
    public static void MapCountryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/country/{countryName}", HandleGetCountryWithStatesAsync)
            .WithName("GetCountryWithStates")
            .WithOpenApi();
    }

    private static async Task<IResult> HandleGetCountryWithStatesAsync(
        string countryName,
        ICountryService countryService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                return Results.BadRequest("Country name cannot be empty.");
            }

            var country = await countryService.GetCountryByNameAsync(countryName);

            if (country is null)
            {
                return Results.NotFound($"Country '{countryName}' not found.");
            }

            var states = await countryService.GetStatesByCountryIdAsync(country.Id);
            var monthlyAqi = await countryService.GetMonthlyAqiByCountryIdAsync(country.Id);
            var result = country.ToDto(states, monthlyAqi);

            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error fetching country data: {ex.Message}");
        }
    }
}
