using LifeDashboard.Data;
using LifeDashboard.Models;

namespace LifeDashboard.Services;

/// <summary>
/// Arguments for when a badge is earned.
/// </summary>
public class BadgeEarnedEventArgs : EventArgs
{
    public Badge Badge { get; set; } = null!;
    public DateTime EarnedAt { get; set; }
}

/// <summary>
/// Manages achievement badges and badge awarding.
/// Thread-safe with proper error handling for MAUI.
/// </summary>
public interface IAchievementService
{
    List<Badge> GetAllBadges();
    bool HasBadge(string badgeId, UserProfile profile);
    Task AwardBadgeAsync(string badgeId, UserProfile profile, CancellationToken cancellationToken = default);
    Task CheckStreakBadgesAsync(int streak, UserProfile profile, CancellationToken cancellationToken = default);
    event EventHandler<BadgeEarnedEventArgs>? BadgeEarned;
}

/// <summary>
/// MAUI-compatible implementation of achievement service.
/// </summary>
public class AchievementService : IAchievementService, IDisposable
{
    private readonly PersistenceHelper _persistence;
    private readonly List<Badge> _badgeDefinitions;
    private readonly object _lockObject = new();
    private bool _disposed;

    public event EventHandler<BadgeEarnedEventArgs>? BadgeEarned;

    public AchievementService(PersistenceHelper persistence)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _badgeDefinitions = InitializeBadgeDefinitions();
    }

    public List<Badge> GetAllBadges()
    {
        lock (_lockObject)
        {
            return new List<Badge>(_badgeDefinitions);
        }
    }

    public bool HasBadge(string badgeId, UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(badgeId);
        ArgumentNullException.ThrowIfNull(profile);

        lock (_lockObject)
        {
            return profile.Badges.Contains(badgeId);
        }
    }

    public async Task AwardBadgeAsync(string badgeId, UserProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(badgeId);
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            lock (_lockObject)
            {
                if (HasBadge(badgeId, profile))
                    return;
            }

            var badge = _badgeDefinitions.FirstOrDefault(b => b.Id == badgeId);
            if (badge == null)
                return;

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    if (!profile.Badges.Contains(badgeId))
                    {
                        profile.Badges.Add(badgeId);
                    }
                }
                _persistence.SaveProfile(profile);
            }, cancellationToken);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                BadgeEarned?.Invoke(this, new BadgeEarnedEventArgs
                {
                    Badge = badge,
                    EarnedAt = DateTime.Now
                });
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error awarding badge {badgeId}: {ex.Message}");
            throw;
        }
    }

    public async Task CheckStreakBadgesAsync(int streak, UserProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        try
        {
            if (streak >= 3 && !HasBadge("streak_3", profile))
                await AwardBadgeAsync("streak_3", profile, cancellationToken);

            if (streak >= 7 && !HasBadge("streak_7", profile))
                await AwardBadgeAsync("streak_7", profile, cancellationToken);

            if (streak >= 14 && !HasBadge("streak_14", profile))
                await AwardBadgeAsync("streak_14", profile, cancellationToken);

            if (streak >= 30 && !HasBadge("streak_30", profile))
                await AwardBadgeAsync("streak_30", profile, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking streak badges: {ex.Message}");
            throw;
        }
    }

    private static List<Badge> InitializeBadgeDefinitions()
    {
        return new List<Badge>
        {
            new() { Id = "first_task", Name = "Task Master", Icon = "??", Description = "Complete your first task" },
            new() { Id = "first_habit", Name = "Habit Starter", Icon = "??", Description = "Complete your first habit" },
            new() { Id = "streak_3", Name = "On Fire!", Icon = "??", Description = "Achieve a 3-day streak" },
            new() { Id = "streak_7", Name = "Week Warrior", Icon = "??", Description = "Achieve a 7-day streak" },
            new() { Id = "streak_14", Name = "Fortnight Fighter", Icon = "??", Description = "Achieve a 14-day streak" },
            new() { Id = "streak_30", Name = "Monthly Legend", Icon = "??", Description = "Achieve a 30-day streak" },
            new() { Id = "100_xp", Name = "Century Club", Icon = "??", Description = "Earn 100 XP" },
            new() { Id = "level_5", Name = "Rising Star", Icon = "?", Description = "Reach level 5" },
            new() { Id = "level_10", Name = "Champion", Icon = "??", Description = "Reach level 10" },
            new() { Id = "perfect_day", Name = "Perfect Day", Icon = "?", Description = "Complete all tasks and habits in a day" }
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            lock (_lockObject)
            {
                _badgeDefinitions?.Clear();
            }
        }
        _disposed = true;
    }

    ~AchievementService() => Dispose(false);
}
