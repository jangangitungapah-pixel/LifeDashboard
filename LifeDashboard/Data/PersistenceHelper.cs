using System.Text.Json;
using LifeDashboard.Models;

namespace LifeDashboard.Data;

/// <summary>
/// Handles JSON persistence for user profile, XP events, and daily stats.
/// MAUI-compatible with async operations and proper error handling.
/// </summary>
public class PersistenceHelper : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _profilePath;
    private readonly string _xpEventsPath;
    private readonly string _dailyStatsPath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public PersistenceHelper()
    {
        var appDataDir = FileSystem.AppDataDirectory;
        _profilePath = Path.Combine(appDataDir, "user_profile.json");
        _xpEventsPath = Path.Combine(appDataDir, "xp_events.json");
        _dailyStatsPath = Path.Combine(appDataDir, "daily_stats.json");
    }

    public UserProfile LoadProfile()
    {
        ThrowIfDisposed();
        try
        {
            if (File.Exists(_profilePath))
            {
                var json = File.ReadAllText(_profilePath);
                return JsonSerializer.Deserialize<UserProfile>(json) ?? CreateDefaultProfile();
            }
            return CreateDefaultProfile();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
            return CreateDefaultProfile();
        }
    }

    public void SaveProfile(UserProfile profile)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(profile);
        try
        {
            var json = JsonSerializer.Serialize(profile, JsonOptions);
            File.WriteAllText(_profilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
            throw;
        }
    }

    public List<XPEvent> LoadXPEvents()
    {
        ThrowIfDisposed();
        try
        {
            if (File.Exists(_xpEventsPath))
            {
                var json = File.ReadAllText(_xpEventsPath);
                return JsonSerializer.Deserialize<List<XPEvent>>(json) ?? new List<XPEvent>();
            }
            return new List<XPEvent>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading XP events: {ex.Message}");
            return new List<XPEvent>();
        }
    }

    public void SaveXPEvents(List<XPEvent> events)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(events);
        try
        {
            var json = JsonSerializer.Serialize(events, JsonOptions);
            File.WriteAllText(_xpEventsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving XP events: {ex.Message}");
            throw;
        }
    }

    public List<DailyStats> LoadDailyStats()
    {
        ThrowIfDisposed();
        try
        {
            if (File.Exists(_dailyStatsPath))
            {
                var json = File.ReadAllText(_dailyStatsPath);
                return JsonSerializer.Deserialize<List<DailyStats>>(json) ?? new List<DailyStats>();
            }
            return new List<DailyStats>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading daily stats: {ex.Message}");
            return new List<DailyStats>();
        }
    }

    public void SaveDailyStats(List<DailyStats> stats)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(stats);
        try
        {
            var json = JsonSerializer.Serialize(stats, JsonOptions);
            File.WriteAllText(_dailyStatsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving daily stats: {ex.Message}");
            throw;
        }
    }

    private static UserProfile CreateDefaultProfile()
    {
        return new UserProfile
        {
            TotalXp = 0,
            Level = 0,
            Badges = new(),
            LastLevelUpDate = DateTime.Now,
            LastLoginDate = DateTime.Now
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
        if (disposing) _semaphore?.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    ~PersistenceHelper() => Dispose(false);
}
