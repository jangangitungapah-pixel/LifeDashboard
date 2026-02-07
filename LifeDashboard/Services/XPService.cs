using LifeDashboard.Data;
using LifeDashboard.Models;
using System.Collections.ObjectModel;

namespace LifeDashboard.Services;

/// <summary>
/// Arguments for the LevelUp event.
/// </summary>
public class LevelUpEventArgs : EventArgs
{
    /// <summary>
    /// The new level achieved.
    /// </summary>
    public int NewLevel { get; set; }

    /// <summary>
    /// Total XP accumulated.
    /// </summary>
    public long TotalXp { get; set; }

    /// <summary>
    /// XP gained in the current level.
    /// </summary>
    public int XpGainedThisLevel { get; set; }
}

/// <summary>
/// Manages XP gain, level computation, and XP-related events.
/// 
/// Level Formula:
/// - XP required for level N = 100 * N * N
/// - Level = floor(sqrt(TotalXp / 100))
/// - Example: Level 1 = 100 XP, Level 2 = 400 XP, Level 3 = 900 XP
/// 
/// XP Sources:
/// - Task completion: +10 XP
/// - Habit completion: +5 XP
/// - Streak milestone (3-day): +10 XP
/// - Streak milestone (7-day): +30 XP
/// - Streak milestone (14-day): +70 XP
/// - Streak milestone (30-day): +200 XP
/// </summary>
public interface IXPService
{
    /// <summary>
    /// Current total XP.
    /// </summary>
    long TotalXp { get; }

    /// <summary>
    /// Current level.
    /// </summary>
    int Level { get; }

    /// <summary>
    /// User profile.
    /// </summary>
    UserProfile Profile { get; }

    /// <summary>
    /// Adds XP asynchronously and checks for level ups.
    /// </summary>
    Task AddXpAsync(int amount, string source, string? relatedId = null, string? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the XP required for a specific level.
    /// </summary>
    long GetXpRequiredForLevel(int level);

    /// <summary>
    /// Gets the XP progress to the next level (0.0 - 1.0).
    /// </summary>
    double GetProgressToNextLevel();

    /// <summary>
    /// Loads profile and events from persistence.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event fired when a level up occurs.
    /// </summary>
    event EventHandler<LevelUpEventArgs>? LevelUp;
}

/// <summary>
/// Default implementation of the XP Service.
/// Thread-safe and MAUI-compatible.
/// </summary>
public class XPService : IXPService, IDisposable
{
    private readonly PersistenceHelper _persistence;
    private readonly object _lockObject = new object();
    private UserProfile _profile = new();
    private List<XPEvent> _xpEvents = new();
    private bool _disposed;

    public long TotalXp
    {
        get
        {
            lock (_lockObject)
            {
                return _profile.TotalXp;
            }
        }
    }

    public int Level
    {
        get
        {
            lock (_lockObject)
            {
                return _profile.Level;
            }
        }
    }

    public UserProfile Profile
    {
        get
        {
            lock (_lockObject)
            {
                return _profile;
            }
        }
    }

    public event EventHandler<LevelUpEventArgs>? LevelUp;

    /// <summary>
    /// Initializes a new instance of the XPService.
    /// </summary>
    public XPService(PersistenceHelper persistence)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    /// <summary>
    /// Initializes the service by loading profile and XP events from persistence.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _profile = _persistence.LoadProfile() ?? new UserProfile();
                    _xpEvents = _persistence.LoadXPEvents() ?? new List<XPEvent>();
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing XPService: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Adds XP asynchronously and checks for level ups.
    /// </summary>
    public async Task AddXpAsync(int amount, string source, string? relatedId = null, string? context = null, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            return;

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be null or empty.", nameof(source));

        try
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    // Record the XP event
                    var xpEvent = new XPEvent
                    {
                        Source = source,
                        Amount = amount,
                        Date = DateTime.Now,
                        RelatedId = relatedId,
                        Context = context
                    };
                    _xpEvents.Add(xpEvent);

                    // Get old level before adding XP
                    int oldLevel = _profile.Level;

                    // Add XP
                    _profile.TotalXp += amount;

                    // Compute new level
                    _profile.Level = GetLevelFromXp(_profile.TotalXp);

                    // Check for level up
                    if (_profile.Level > oldLevel)
                    {
                        _profile.LastLevelUpDate = DateTime.Now;
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LevelUp?.Invoke(this, new LevelUpEventArgs
                            {
                                NewLevel = _profile.Level,
                                TotalXp = _profile.TotalXp,
                                XpGainedThisLevel = (int)(GetXpAtLevel(oldLevel + 1) - GetXpAtLevel(oldLevel))
                            });
                        });
                    }

                    // Persist
                    _persistence.SaveProfile(_profile);
                    _persistence.SaveXPEvents(_xpEvents);
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding XP: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets the XP required for a specific level.
    /// </summary>
    public long GetXpRequiredForLevel(int level)
    {
        if (level < 0)
            throw new ArgumentException("Level cannot be negative.", nameof(level));

        return 100L * level * level;
    }

    /// <summary>
    /// Gets the XP progress to the next level (0.0 - 1.0).
    /// </summary>
    public double GetProgressToNextLevel()
    {
        lock (_lockObject)
        {
            long currentLevelXp = GetXpAtLevel(_profile.Level);
            long nextLevelXp = GetXpAtLevel(_profile.Level + 1);
            long xpIntoCurrentLevel = _profile.TotalXp - currentLevelXp;
            long xpNeededForLevel = nextLevelXp - currentLevelXp;

            if (xpNeededForLevel <= 0)
                return 0;

            return Math.Min(1.0, (double)xpIntoCurrentLevel / xpNeededForLevel);
        }
    }

    /// <summary>
    /// Gets total XP at a given level.
    /// </summary>
    private static long GetXpAtLevel(int level)
    {
        if (level <= 0)
            return 0;

        return 100L * level * level;
    }

    /// <summary>
    /// Computes level from total XP using the formula: level = floor(sqrt(totalXp / 100))
    /// </summary>
    private static int GetLevelFromXp(long totalXp)
    {
        if (totalXp < 100)
            return 0;

        return (int)Math.Floor(Math.Sqrt(totalXp / 100.0));
    }

    /// <summary>
    /// Disposes of the XPService resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected disposal method.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            lock (_lockObject)
            {
                _xpEvents?.Clear();
                _xpEvents = null!;
                _profile = null!;
            }
        }

        _disposed = true;
    }

    /// <summary>
    /// Finalizer for resource cleanup.
    /// </summary>
    ~XPService()
    {
        Dispose(false);
    }
}
