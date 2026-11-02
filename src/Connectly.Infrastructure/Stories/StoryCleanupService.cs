using Connectly.Application.Common.Interfaces;
using Connectly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Connectly.Infrastructure.Stories;

/// <summary>
/// Housekeeping only: queries already hide expired stories, this job reclaims the rows and image files.
/// </summary>
public class StoryCleanupService(IServiceScopeFactory scopes, TimeProvider clock, ILogger<StoryCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try { await CleanAsync(stoppingToken); }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Story cleanup failed; will retry");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) { /* shutting down */ }
    }

    private async Task CleanAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var now = clock.GetUtcNow().UtcDateTime;

        List<Domain.Entities.Story> expired;
        do
        {
            expired = await db.Stories.Where(s => s.ExpiresAtUtc < now).OrderBy(s => s.ExpiresAtUtc).Take(200).ToListAsync(ct);
            foreach (var story in expired.Where(s => s.ImageUrl is not null))
                await storage.DeleteAsync(story.ImageUrl!, ct);

            db.Stories.RemoveRange(expired);
            await db.SaveChangesAsync(ct);
        }
        while (expired.Count == 200);
    }
}
