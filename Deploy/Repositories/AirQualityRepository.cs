using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Repositories;

public class AirQualityRepository : IAirQualityRepository
{
    private readonly string _connectionString;

    public AirQualityRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Database connection string 'Postgres' is not configured.");
    }

    public async Task EnsureTableExistsAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string createSql = @"
            CREATE TABLE IF NOT EXISTS air_quality_readings (
                id BIGSERIAL PRIMARY KEY,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                latitude DOUBLE PRECISION NOT NULL,
                longitude DOUBLE PRECISION NOT NULL,
                hourly TEXT NOT NULL,
                payload JSONB NOT NULL
            );";

        await using (var createCommand = new NpgsqlCommand(createSql, connection))
        {
            await createCommand.ExecuteNonQueryAsync();
        }

        // Backwards-compatible: if table already existed with fewer columns, add them.
        const string alterSql = @"
            ALTER TABLE air_quality_readings ADD COLUMN IF NOT EXISTS latitude DOUBLE PRECISION;
            ALTER TABLE air_quality_readings ADD COLUMN IF NOT EXISTS longitude DOUBLE PRECISION;
            ALTER TABLE air_quality_readings ADD COLUMN IF NOT EXISTS hourly TEXT;";

        await using var alterCommand = new NpgsqlCommand(alterSql, connection);
        await alterCommand.ExecuteNonQueryAsync();
    }

    public async Task InsertReadingAsync(double latitude, double longitude, string hourly, string payload)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO air_quality_readings (latitude, longitude, hourly, payload)
            VALUES (@latitude, @longitude, @hourly, CAST(@payload AS jsonb));";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("latitude", latitude);
        command.Parameters.AddWithValue("longitude", longitude);
        command.Parameters.AddWithValue("hourly", hourly);
        command.Parameters.AddWithValue("payload", payload);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<string?> GetLatestReadingAsync(double latitude, double longitude, string hourly)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT payload
            FROM air_quality_readings
            WHERE latitude = @latitude
              AND longitude = @longitude
              AND hourly = @hourly
            ORDER BY created_at DESC
            LIMIT 1;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("latitude", latitude);
        command.Parameters.AddWithValue("longitude", longitude);
        command.Parameters.AddWithValue("hourly", hourly);
        var result = await command.ExecuteScalarAsync();
        return result?.ToString();
    }
}

