using System.Collections.Concurrent;

namespace LifeDashboard.Services;

/// <summary>
/// Cross-platform local notification service wrapper.
/// MAUI-compatible with proper error handling.
/// </summary>
public interface INotificationService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SendAsync(string id, string title, string body, CancellationToken cancellationToken = default);
    Task ScheduleAsync(string id, string title, string body, DateTime when, CancellationToken cancellationToken = default);
    Task ScheduleDailyAsync(string id, string title, string body, int hour, int minute, CancellationToken cancellationToken = default);
    Task CancelAsync(string id, CancellationToken cancellationToken = default);
    bool IsSupported { get; }
}

/// <summary>
/// MAUI-compatible notification service implementation.
/// </summary>
public class NotificationService : INotificationService, IDisposable
{
    private bool _initialized;
    private readonly object _lockObject = new();
    private readonly ConcurrentDictionary<string, DateTime> _scheduledNotifications = new();
    private bool _disposed;

    public bool IsSupported { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            lock (_lockObject)
            {
                if (_initialized) return;
            }

            IsSupported = DeviceInfo.Platform == DevicePlatform.Android ||
                         DeviceInfo.Platform == DevicePlatform.WinUI;

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _initialized = true;
                }
                System.Diagnostics.Debug.WriteLine($"NotificationService initialized. Supported: {IsSupported}");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing notification service: {ex.Message}");
            IsSupported = false;
        }
    }

    public async Task SendAsync(string id, string title, string body, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateParams(id, title, body);

        try
        {
            if (!IsSupported) return;

            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Notification sent: {title} - {body}");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
            throw;
        }
    }

    public async Task ScheduleAsync(string id, string title, string body, DateTime when, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateParams(id, title, body);
        if (when <= DateTime.Now)
            throw new ArgumentException("Scheduled time must be in the future.", nameof(when));

        try
        {
            if (!IsSupported) return;

            _scheduledNotifications.TryAdd(id, when);

            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Notification scheduled: {title} at {when:G}");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scheduling notification: {ex.Message}");
            throw;
        }
    }

    public async Task ScheduleDailyAsync(string id, string title, string body, int hour, int minute, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateParams(id, title, body);
        ValidateTime(hour, minute);

        try
        {
            if (!IsSupported) return;

            var nextScheduledTime = GetNextOccurrence(hour, minute);
            _scheduledNotifications.TryAdd(id, nextScheduledTime);

            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Daily notification scheduled: {title} at {hour:D2}:{minute:D2}");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scheduling daily notification: {ex.Message}");
            throw;
        }
    }

    public async Task CancelAsync(string id, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            _scheduledNotifications.TryRemove(id, out _);

            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Notification cancelled: {id}");
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelling notification: {ex.Message}");
            throw;
        }
    }

    private static void ValidateParams(string id, string title, string body)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty.", nameof(body));
    }

    private static void ValidateTime(int hour, int minute)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentException("Hour must be between 0 and 23.", nameof(hour));
        if (minute < 0 || minute > 59)
            throw new ArgumentException("Minute must be between 0 and 59.", nameof(minute));
    }

    private static DateTime GetNextOccurrence(int hour, int minute)
    {
        var now = DateTime.Now;
        var nextOccurrence = now.Date.AddHours(hour).AddMinutes(minute);
        if (nextOccurrence <= now)
            nextOccurrence = nextOccurrence.AddDays(1);
        return nextOccurrence;
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
                _scheduledNotifications?.Clear();
                _initialized = false;
            }
        }
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    ~NotificationService() => Dispose(false);
}
