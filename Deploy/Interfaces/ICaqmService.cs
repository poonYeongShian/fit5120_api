namespace Deploy.Interfaces;

public interface ICaqmService
{
    Task<string> FetchAndStoreCurrentReadingAsync();
    Task<string?> GetLatestStoredReadingAsync();
}
