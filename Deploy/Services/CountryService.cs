using Deploy.Interfaces;
using Deploy.Models;

namespace Deploy.Services;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _repository;

    public CountryService(ICountryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MonthlyAqi>> GetMonthlyAqiByCountryIdAsync(int countryId)
    {
        if (countryId <= 0)
        {
            throw new ArgumentException("Country ID must be greater than 0.", nameof(countryId));
        }

        return await _repository.GetMonthlyAqiByCountryIdAsync(countryId);
    }

    public async Task<Country?> GetCountryByNameAsync(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            throw new ArgumentException("Country name cannot be empty.", nameof(countryName));
        }

        return await _repository.GetCountryByNameAsync(countryName);
    }

    public async Task<List<CountryState>> GetStatesByCountryIdAsync(int countryId)
    {
        if (countryId <= 0)
        {
            throw new ArgumentException("Country ID must be greater than 0.", nameof(countryId));
        }

        return await _repository.GetStatesByCountryIdAsync(countryId);
    }
}
