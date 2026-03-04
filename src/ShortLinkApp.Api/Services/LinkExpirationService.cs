namespace ShortLinkApp.Api.Services;

/// <summary>
/// Background service that periodically marks expired links as inactive so that
/// the redirection endpoint returns the correct status without requiring real-time
/// expiry checks on every request.
/// </summary>
public sealed class LinkExpirationService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<LinkExpirationService> logger,
    IConfiguration configuration) : BackgroundService
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaxInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuredInterval = configuration.GetValue<TimeSpan?>("LinkExpiration:CheckInterval") ?? DefaultInterval;
        var interval = configuredInterval < MinInterval ? MinInterval
            : configuredInterval > MaxInterval ? MaxInterval
            : configuredInterval;

        if (interval != configuredInterval)
        {
            logger.LogWarning(
                "Configured LinkExpiration:CheckInterval ({Configured}) is out of range [{Min}, {Max}]. Using {Actual}.",
                configuredInterval, MinInterval, MaxInterval, interval);
        }

        logger.LogInformation("LinkExpirationService started. Check interval: {Interval}.", interval);

        // Run immediately on startup to process links that expired while the service was stopped.
        await DeactivateExpiredLinksAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DeactivateExpiredLinksAsync(stoppingToken);
        }
    }

    private async Task DeactivateExpiredLinksAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<ILinkRepository>();

            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var count = await repository.DeactivateExpiredLinksAsync(utcNow, cancellationToken);

            if (count > 0)
            {
                logger.LogInformation(
                    "LinkExpirationService deactivated {Count} expired link(s) at {UtcNow}.",
                    count, utcNow);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "LinkExpirationService encountered an error while deactivating expired links.");
        }
    }
}
