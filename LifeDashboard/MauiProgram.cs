using Microsoft.Extensions.Logging;
using LifeDashboard.Data;
using LifeDashboard.Services;
using LifeDashboard.Views;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm;

namespace LifeDashboard
{
    /// <summary>
    /// Configures and initializes the MAUI application with dependency injection.
    /// </summary>
    public static class MauiProgram
    {
    /// <summary>
    /// Creates and configures the MauiApp with services, fonts, and platform-specific settings.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureServices()
            .ConfigureLogging();

        return builder.Build();
    }

    /// <summary>
    /// Registers all application services with the dependency injection container.
    /// </summary>
    private static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        // Data & Persistence
        builder.Services.AddSingleton<PersistenceHelper>();

        // Core Services
        builder.Services.AddSingleton<IXPService, XPService>();
        builder.Services.AddSingleton<IAchievementService, AchievementService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();

        // AI Service with dependency
        builder.Services.AddSingleton<IAIService>(sp =>
        {
            var aiService = new AIService(new LocalAIStub());
            return aiService;
        });

        // Views
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder;
    }

    /// <summary>
    /// Configures logging for the application.
    /// </summary>
    private static MauiAppBuilder ConfigureLogging(this MauiAppBuilder builder)
    {
#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif
        return builder;
    }
    }
}


