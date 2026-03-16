# FIT5120 Web API

A **.NET 9 Minimal API** backend that powers air-quality data for the FIT5120 project. It integrates with two external data sources (Malaysia's CAQM feed and Open-Meteo) and stores readings in a **PostgreSQL** database.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running Locally](#running-locally)
- [Running with Docker](#running-with-docker)
- [API Endpoints](#api-endpoints)
- [Database Schema](#database-schema)

---

## Features

- Fetch and persist live **CAQM** (Continuous Air Quality Monitoring) readings from Malaysia's DOE public API.
- Query **Open-Meteo** air-quality data (PM10, PM2.5, etc.) by latitude/longitude with automatic DB caching and mock-data fallback.
- Retrieve **country and state** metadata along with **monthly AQI trend** statistics for Malaysia.
- Swagger/OpenAPI UI available in the Development environment.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 / ASP.NET Core Minimal APIs |
| Database | PostgreSQL (via **Npgsql** + **Dapper**) |
| External APIs | Malaysia DOE CAQM, Open-Meteo Air-Quality API |
| Documentation | Swashbuckle (Swagger UI) |
| Container | Docker |

---

## Project Structure

```
fit5120_web_api/
??? Deploy/
?   ??? Endpoints/          # Minimal API route handlers
?   ?   ??? CaqmEndpoints.cs
?   ?   ??? AirQualityEndpoints.cs
?   ?   ??? CountryEndpoints.cs
?   ??? Services/           # Business logic
?   ?   ??? CaqmService.cs
?   ?   ??? AirQualityService.cs
?   ?   ??? CountryService.cs
?   ??? Repositories/       # PostgreSQL data access
?   ?   ??? CaqmRepository.cs
?   ?   ??? AirQualityRepository.cs
?   ?   ??? CountryRepository.cs
?   ??? Interfaces/         # Abstractions (service & repository contracts)
?   ??? Models/             # Domain models (CaqmReading, Country, MonthlyAqi, …)
?   ??? Dtos/               # Response DTOs
?   ??? Mappings/           # Model ? DTO mapping helpers
?   ??? Program.cs          # App entry point & DI registration
?   ??? appsettings.json    # App configuration
?   ??? Deploy.csproj
??? Dockerfile
??? README.md
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/) (v14+ recommended)
- [Docker](https://www.docker.com/) *(optional, for containerised deployment)*

---

## Configuration

All settings live in `Deploy/appsettings.json`. Replace the placeholder values before running:

```json
{
  "ConnectionStrings": {
    "Postgres": "<your-postgresql-connection-string>"
  },
  "Caqm": {
    "ApiUrl": "https://eqms.doe.gov.my/api3/publicmapproxy/..."
  },
  "AirQuality": {
    "ApiBaseUrl": "https://air-quality-api.open-meteo.com/v1/air-quality"
  }
}
```

| Key | Description |
|-----|-------------|
| `ConnectionStrings:Postgres` | Full Npgsql connection string (e.g. `Host=localhost;Database=fit5120;Username=postgres;Password=…`) |
| `Caqm:ApiUrl` | Malaysia DOE public CAQM endpoint (pre-filled in `appsettings.json`) |
| `AirQuality:ApiBaseUrl` | Open-Meteo base URL (pre-filled in `appsettings.json`) |

> **Tip:** Use an `appsettings.Development.json` file or environment variables to override secrets locally without touching the committed file.

---

## Running Locally

```bash
# 1. Restore packages
dotnet restore Deploy/Deploy.csproj

# 2. Run the API (Development mode – Swagger enabled)
cd Deploy
dotnet run
```

The API will be available at:

- HTTP  ? `http://localhost:5119`
- HTTPS ? `https://localhost:7244`
- Swagger UI ? `http://localhost:5119/swagger`

---

## Running with Docker

```bash
# Build the image
docker build -t fit5120-api .

# Run the container (pass your connection string as an env variable)
docker run -p 8080:8080 \
  -e ConnectionStrings__Postgres="Host=host.docker.internal;Database=fit5120;Username=postgres;Password=yourpassword" \
  fit5120-api
```

The API will be accessible at `http://localhost:8080`.

---

## API Endpoints

### CAQM (Malaysia Continuous Air Quality Monitoring)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/caqm/current` | Fetches the latest reading from the DOE public API and stores it in the database. Returns the raw JSON response. |
| `GET` | `/api/caqm/latest` | Returns the most recently stored CAQM reading from the database. |

### Air Quality (Open-Meteo)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/air-quality` | Fetches hourly air-quality data for the given coordinates. Falls back to the latest cached DB reading, then to a mock response if the external API is unavailable. |

**Query parameters for `/api/air-quality`:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `latitude` | `double` | `52.52` | Latitude of the location |
| `longitude` | `double` | `13.41` | Longitude of the location |
| `hourly` | `string` | `pm10,pm2_5` | Comma-separated list of hourly variables (e.g. `pm10,pm2_5`) |

### Country & States

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/country/malaysia` | Returns Malaysia's metadata, a list of its states, and monthly AQI trend data per state. |

---

## Database Schema

Tables are created automatically on first use (no manual migration needed).

### `caqm_readings`

| Column | Type | Description |
|--------|------|-------------|
| `id` | `BIGSERIAL` | Primary key |
| `created_at` | `TIMESTAMPTZ` | Timestamp of insertion (default: `NOW()`) |
| `payload` | `JSONB` | Raw JSON response from the CAQM API |

### `air_quality_readings`

| Column | Type | Description |
|--------|------|-------------|
| `id` | `BIGSERIAL` | Primary key |
| `created_at` | `TIMESTAMPTZ` | Timestamp of insertion (default: `NOW()`) |
| `latitude` | `DOUBLE PRECISION` | Query latitude |
| `longitude` | `DOUBLE PRECISION` | Query longitude |
| `hourly` | `TEXT` | Requested hourly fields (e.g. `pm10,pm2_5`) |
| `payload` | `JSONB` | Raw JSON response from the Open-Meteo API |

> The `general_country`, `general_country_state`, and `v_monthly_trend` tables/views are expected to be pre-populated in your PostgreSQL database.
