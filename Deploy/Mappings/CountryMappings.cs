using Deploy.Dtos;
using Deploy.Models;

namespace Deploy.Mappings;

public static class CountryMappings
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            Id = country.Id,
            Name = country.CountryName
        };
    }

    public static CountryStateDto ToDto(this CountryState state, List<MonthlyAqi> monthlyAqiData)
    {
        return new CountryStateDto
        {
            Id = state.Id,
            Name = state.StateName,
            MonthlyAqi = monthlyAqiData
                .Where(a => a.StateId == state.Id)
                .OrderBy(a => a.Year)
                .ThenBy(a => a.Month)
                .Select(a => new MonthlyAqiDto
                {
                    Year          = a.Year,
                    Month         = a.Month,
                    AvgAqi        = Math.Round(a.AverageAqiValue, 2),
                    PeakAqi       = Math.Round(a.PeakAqiValue, 2),
                    WorsenigDays  = a.WorsenigDays,
                    ImprovingDays = a.ImprovingDays
                })
                .ToList()
        };
    }

    public static List<CountryStateDto> ToDto(this List<CountryState> states, List<MonthlyAqi> monthlyAqiData)
    {
        return states.Select(s => s.ToDto(monthlyAqiData)).ToList();
    }

    public static CountryWithStatesResponseDto ToDto(this Country country, List<CountryState> states, List<MonthlyAqi> monthlyAqiData)
    {
        return new CountryWithStatesResponseDto
        {
            Country = country.ToDto(),
            States = states.ToDto(monthlyAqiData)
        };
    }
}
