using System.Net.Http;
using System.Text.Json;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/caqm/current", async (IHttpClientFactory httpClientFactory, IConfiguration configuration) =>
{
    var apiUrl = configuration["Caqm:ApiUrl"];
    var connectionString = configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("API URL or database connection string is not configured.");
    }

    var httpClient = httpClientFactory.CreateClient();

    HttpResponseMessage response;
    try
    {
        response = await httpClient.GetAsync(apiUrl);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error calling external API: {ex.Message}");
    }

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"External API returned status code {(int)response.StatusCode}.");
    }

    var content = await response.Content.ReadAsStringAsync();

    try
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Ensure table exists
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS caqm_readings (
                id BIGSERIAL PRIMARY KEY,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                payload JSONB NOT NULL
            );";

        await using (var createTableCommand = new NpgsqlCommand(createTableSql, connection))
        {
            await createTableCommand.ExecuteNonQueryAsync();
        }

        const string insertSql = @"
            INSERT INTO caqm_readings (payload)
            VALUES (CAST(@payload AS jsonb));";

        await using (var insertCommand = new NpgsqlCommand(insertSql, connection))
        {
            insertCommand.Parameters.AddWithValue("payload", content);
            await insertCommand.ExecuteNonQueryAsync();
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error saving data to PostgreSQL: {ex.Message}");
    }

    return Results.Content(content, "application/json");
})
.WithName("GetCurrentCaqmReading")
.WithOpenApi();

app.MapGet("/api/caqm/latest", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("Database connection string is not configured.");
    }

    try
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        const string selectSql = @"
            SELECT payload
            FROM caqm_readings
            ORDER BY created_at DESC
            LIMIT 1;";

        await using var selectCommand = new NpgsqlCommand(selectSql, connection);
        var result = await selectCommand.ExecuteScalarAsync();

        if (result is null)
        {
            return Results.NotFound("No readings stored yet.");
        }

        var json = result.ToString() ?? "{}";
        return Results.Content(json, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error reading data from PostgreSQL: {ex.Message}");
    }
})
.WithName("GetLatestStoredCaqmReading")
.WithOpenApi();

app.Run();