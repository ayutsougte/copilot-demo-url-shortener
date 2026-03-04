using Microsoft.EntityFrameworkCore;
using ShortLinkApp.Api.Data;
using ShortLinkApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ILinkRepository, LinkRepository>();
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddScoped<IClickTrackingService, ClickTrackingService>();

var app = builder.Build();

// Apply pending migrations automatically on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/{shortCode}", async (
        string shortCode,
        ILinkRepository linkRepository,
        IClickTrackingService clickTrackingService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
        var link = await linkRepository.GetByShortCodeAsync(shortCode, cancellationToken);

        if (link is null)
            return Results.NotFound();

        if (!link.IsActive || link.IsExpired(timeProvider.GetUtcNow().UtcDateTime))
            return Results.StatusCode(StatusCodes.Status410Gone);

        var referrer = httpContext.Request.Headers.Referer.ToString();
        await clickTrackingService.RecordClickAsync(link.Id, string.IsNullOrEmpty(referrer) ? null : referrer, cancellationToken);

        return Results.Redirect(link.OriginalUrl, permanent: false);
    })
    .WithName("RedirectShortLink");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
