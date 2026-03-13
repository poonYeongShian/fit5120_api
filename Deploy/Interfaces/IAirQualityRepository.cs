namespace Deploy.Interfaces;

public interface IAirQualityRepository
{
    Task EnsureTableExistsAsync();
    Task InsertReadingAsync(double latitude, double longitude, string hourly, string payload);
    Task<string?> GetLatestReadingAsync(double latitude, double longitude, string hourly);
}

