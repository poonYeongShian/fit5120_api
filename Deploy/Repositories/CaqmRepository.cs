using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Repositories;

public class CaqmRepository : ICaqmRepository
{
    private readonly string _connectionString;

    public CaqmRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Database connection string 'Postgres' is not configured.");
    }

    public async Task EnsureTableExistsAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            CREATE TABLE IF NOT EXISTS caqm_readings (
                id BIGSERIAL PRIMARY KEY,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                payload JSONB NOT NULL
            );";

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InsertReadingAsync(string payload)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO caqm_readings (payload)
            VALUES (CAST(@payload AS jsonb));";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("payload", payload);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<string?> GetLatestReadingAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT payload
            FROM caqm_readings
            ORDER BY created_at DESC
            LIMIT 1;";

        await using var command = new NpgsqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();

        return result?.ToString();
    }
}
