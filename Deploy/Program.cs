using Deploy.Endpoints;
using Deploy.Interfaces;
using Deploy.Repositories;
using Deploy.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICaqmRepository, CaqmRepository>();
builder.Services.AddScoped<ICaqmService, CaqmService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
app.MapCaqmEndpoints();

app.Run();