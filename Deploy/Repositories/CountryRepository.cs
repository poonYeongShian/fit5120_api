using Dapper;
using Deploy.Interfaces;
using Deploy.Models;
using Npgsql;

namespace Deploy.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly string _connectionString;

    public CountryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Database connection string 'Postgres' is not configured.");
    }

    public async Task<Country?> GetCountryByNameAsync(string countryName)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT id,
                   country_name  AS CountryName,
                   iso2_code     AS Iso2Code,
                   iso3_code     AS Iso3Code,
                   continent,
                   currency_code AS CurrencyCode,
                   currency_symbol AS CurrencySymbol,
                   phone_code    AS PhoneCode,
                   country_domain AS CountryDomain,
                   nationality,
                   currency_name  AS CurrencyName
            FROM general_country
            WHERE LOWER(country_name) = LOWER(@countryName)
            LIMIT 1;";

        return await connection.QueryFirstOrDefaultAsync<Country>(sql, new { countryName });
    }

    public async Task<List<CountryState>> GetStatesByCountryIdAsync(int countryId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT id,
                   state_name  AS StateName,
                   country_id  AS CountryId,
                   state_code  AS StateCode
            FROM general_country_state
            WHERE country_id = @countryId
            ORDER BY state_name;";

        var results = await connection.QueryAsync<CountryState>(sql, new { countryId });
        return results.AsList();
    }

    public async Task<List<MonthlyAqi>> GetMonthlyAqiByCountryIdAsync(int countryId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT vt.state_id                             AS StateId,
                   EXTRACT(YEAR  FROM vt.month)::integer   AS Year,
                   EXTRACT(MONTH FROM vt.month)::integer   AS Month,
                   vt.avg_aqi                              AS AverageAqiValue,
                   vt.peak_aqi                             AS PeakAqiValue,
                   vt.worsening_days                       AS WorsenigDays,
                   vt.improving_days                       AS ImprovingDays
            FROM v_monthly_trend vt
            INNER JOIN general_country_state gcs ON gcs.id = vt.state_id
            WHERE gcs.country_id = @countryId
            ORDER BY vt.state_id, year, month;";

        var results = await connection.QueryAsync<MonthlyAqi>(sql, new { countryId });
        return results.AsList();
    }
}
