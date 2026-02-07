using LifeDashboard.Models;

namespace LifeDashboard.Services;

/// <summary>
/// Interface for coaching tip providers (local or remote).
/// </summary>
public interface IAIAdapter
{
    /// <summary>
    /// Gets a coaching tip asynchronously based on context.
    /// </summary>
    Task<CoachTip> GetTipAsync(string context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Local stub implementation that returns canned coaching tips.
/// </summary>
public class LocalAIStub : IAIAdapter
{
    private static readonly List<CoachTip> _tips = new()
    {
        new CoachTip
        {
            Tip = "Try the Pomodoro Technique: 25 min work, 5 min break.",
            Motivation = "Small consistent steps build lasting habits! ??",
            Source = "local"
        },
        new CoachTip
        {
            Tip = "Start your day by completing your hardest task first.",
            Motivation = "You're stronger than your excuses! ??",
            Source = "local"
        },
        new CoachTip
        {
            Tip = "Break big goals into micro-habits (2-min chunks).",
            Motivation = "Momentum builds success! ??",
            Source = "local"
        },
        new CoachTip
        {
            Tip = "Review your progress weekly and celebrate wins.",
            Motivation = "Every step forward counts! ?",
            Source = "local"
        },
        new CoachTip
        {
            Tip = "Stack habits: do new habit after existing routine.",
            Motivation = "Habit stacking accelerates progress! ??",
            Source = "local"
        },
        new CoachTip
        {
            Tip = "Keep it simple—one new habit at a time.",
            Motivation = "Mastery comes from focus and consistency. ??",
            Source = "local"
        }
    };

    /// <summary>
    /// Gets a random tip from the collection.
    /// </summary>
    public Task<CoachTip> GetTipAsync(string context, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<CoachTip>(cancellationToken);

        var tip = _tips[Random.Shared.Next(_tips.Count)];
        return Task.FromResult(tip);
    }
}

/// <summary>
/// Service for getting AI-powered coaching tips.
/// Pluggable adapter pattern allows swapping between local and remote providers.
/// MAUI-compatible with proper error handling and cancellation support.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Gets a coaching tip based on context.
    /// </summary>
    Task<CoachTip> GetTipAsync(string context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the AI adapter (local or remote).
    /// </summary>
    void SetAdapter(IAIAdapter adapter);
}

/// <summary>
/// MAUI-compatible AI service implementation.
/// </summary>
public class AIService : IAIService, IDisposable
{
    private IAIAdapter _adapter;
    private readonly object _lockObject = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AIService.
    /// </summary>
    public AIService(IAIAdapter adapter)
    {
        _adapter = adapter ?? new LocalAIStub();
    }

    /// <summary>
    /// Gets a coaching tip based on context.
    /// </summary>
    public async Task<CoachTip> GetTipAsync(string context, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(context))
            throw new ArgumentException("Context cannot be null or empty.", nameof(context));

        try
        {
            lock (_lockObject)
            {
                if (_adapter == null)
                    throw new InvalidOperationException("AI Adapter is not configured.");
            }

            return await _adapter.GetTipAsync(context, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("GetTipAsync was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting AI tip: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Sets the AI adapter (local or remote).
    /// </summary>
    public void SetAdapter(IAIAdapter adapter)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(adapter);

        lock (_lockObject)
        {
            _adapter = adapter;
        }
    }

    /// <summary>
    /// Disposes of the service resources.
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
                _adapter = null!;
            }
        }

        _disposed = true;
    }

    /// <summary>
    /// Throws if the service has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>
    /// Finalizer for resource cleanup.
    /// </summary>
    ~AIService()
    {
        Dispose(false);
    }
}
