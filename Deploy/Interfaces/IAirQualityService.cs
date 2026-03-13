namespace Deploy.Interfaces;

public interface IAirQualityService
{
    Task<string> GetAirQualityJsonAsync(double latitude, double longitude, string? hourly);
}

