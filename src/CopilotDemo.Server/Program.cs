using System.Collections.Concurrent;
using System.Text.Json;
using CopilotDemo.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:7288")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

var store = new ConcurrentDictionary<string, ShortLink>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/links", (ShortLink input) =>
{
    var link = new ShortLink
    {
        Id = Guid.NewGuid(),
        Key = input.Key,
        Url = input.Url,
        CreatedAt = DateTimeOffset.UtcNow
    };
    store[link.Key] = link;
    return Results.Created($"/api/links/{link.Key}", link);
});

app.MapGet("/api/links/{key}", (string key) =>
    store.TryGetValue(key, out var link) ? Results.Ok(link) : Results.NotFound());

app.Run();
