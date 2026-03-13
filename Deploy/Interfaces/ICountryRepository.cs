using Deploy.Models;

namespace Deploy.Interfaces;

public interface ICountryRepository
{
    Task<Country?> GetCountryByNameAsync(string countryName);
    Task<List<CountryState>> GetStatesByCountryIdAsync(int countryId);
    Task<List<MonthlyAqi>> GetMonthlyAqiByCountryIdAsync(int countryId);
}
