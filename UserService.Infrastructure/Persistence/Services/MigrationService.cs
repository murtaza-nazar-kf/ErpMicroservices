using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure.Persistence.DbContexts;

namespace UserService.Infrastructure.Persistence.Services;

public class MigrationService(ILogger<MigrationService> logger, IServiceProvider serviceProvider)
{
    private const int MaxRetries = 10;
    private const int RetryDelaySeconds = 5;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return ApplyMigrationsWithRetryAsync(MaxRetries, RetryDelaySeconds, cancellationToken);
    }

    private async Task ApplyMigrationsWithRetryAsync(int maxRetries, int retryDelaySeconds, CancellationToken cancellationToken)
    {
        for (var retryCount = 1; retryCount <= maxRetries; retryCount++)
        {
            if (await TryApplyMigrationsAsync(retryCount, maxRetries, cancellationToken).ConfigureAwait(false))
                return;

            if (retryCount < maxRetries)
                await WaitBeforeRetryAsync(retryDelaySeconds, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> TryApplyMigrationsAsync(int retryCount, int maxRetries, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Checking for pending migrations...");

            if (!await HasPendingMigrationsAsync(cancellationToken).ConfigureAwait(false))
            {
                logger.LogInformation("✅ Database is already up-to-date. No new migrations to apply");
                return true; // Exit early if no pending migrations
            }

            logger.LogInformation("Attempting to apply migrations...");
            await ApplyMigrationsAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("✅ Migrations applied successfully");
            return true;
        }
        catch (Exception ex)
        {
            LogMigrationFailure(retryCount, maxRetries, ex);
            return false;
        }
    }

    private async Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        return pendingMigrations.Any();
    }

    private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    private void LogMigrationFailure(int retryCount, int maxRetries, Exception ex)
    {
        logger.LogWarning("Attempt {RetryCount}/{MaxRetries} - Error applying migrations: {ExMessage}",
            retryCount, maxRetries, ex.Message);

        if (retryCount >= maxRetries)
            logger.LogError("Failed to apply migrations after {MaxRetries} attempts: {ExMessage}", maxRetries, ex.Message);
    }

    private Task WaitBeforeRetryAsync(int retryDelaySeconds, CancellationToken cancellationToken)
    {
        logger.LogInformation("Waiting {RetryDelaySeconds} seconds before next attempt...", retryDelaySeconds);
        return Task.Delay(retryDelaySeconds * 1000, cancellationToken);
    }
}
