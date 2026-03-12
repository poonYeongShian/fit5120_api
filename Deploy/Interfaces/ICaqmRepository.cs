namespace Deploy.Interfaces;

public interface ICaqmRepository
{
    Task EnsureTableExistsAsync();
    Task InsertReadingAsync(string payload);
    Task<string?> GetLatestReadingAsync();
}
