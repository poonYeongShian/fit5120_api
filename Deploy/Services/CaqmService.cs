using Deploy.Interfaces;

namespace Deploy.Services;

public class CaqmService : ICaqmService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICaqmRepository _repository;
    private readonly string _apiUrl;

    public CaqmService(
        IHttpClientFactory httpClientFactory,
        ICaqmRepository repository,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _repository = repository;
        _apiUrl = configuration["Caqm:ApiUrl"]
            ?? throw new InvalidOperationException("Caqm:ApiUrl is not configured.");
    }

    public async Task<string> FetchAndStoreCurrentReadingAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(_apiUrl);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        await _repository.EnsureTableExistsAsync();
        await _repository.InsertReadingAsync(content);

        return content;
    }

    public async Task<string?> GetLatestStoredReadingAsync()
    {
        return await _repository.GetLatestReadingAsync();
    }
}
