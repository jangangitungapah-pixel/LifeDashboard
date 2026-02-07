using LifeDashboard.Models;

namespace LifeDashboard.Views;

public partial class HeatmapView : ContentView
{
    /// <summary>
    /// Displays a calendar heatmap of daily completion statistics (last 90 days).
    /// Similar to GitHub contribution graph with enhanced styling.
    /// </summary>
    public HeatmapView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Renders the heatmap with daily stats and updates streak counter.
    /// </summary>
    public void Render(List<DailyStats> allStats)
    {
        HeatmapGrid.Children.Clear();

        var today = DateTime.Today;
        var ninetyDaysAgo = today.AddDays(-89);

        var statsDict = allStats.ToDictionary(s => s.Date);

        // 13 weeks x 7 days
        int dayIndex = 0;
        int maxIntensity = 0;
        
        for (int week = 0; week < 13; week++)
        {
            for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
            {
                var date = ninetyDaysAgo.AddDays(dayIndex);

                if (date > today) break;

                var statsKey = date.ToString("yyyy-MM-dd");
                var intensity = statsDict.ContainsKey(statsKey) ? statsDict[statsKey].Intensity : 0.0;

                if (intensity > 0) maxIntensity = 1;

                var cellColor = GetHeatmapColor(intensity);
                var cell = new Frame
                {
                    CornerRadius = 6,
                    BackgroundColor = cellColor,
                    BorderColor = Colors.Transparent,
                    Padding = 0,
                    HasShadow = false,
                    WidthRequest = 18,
                    HeightRequest = 18
                };

                // Add tap gesture to show daily details
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnCellTapped(date, statsDict.ContainsKey(statsKey) ? statsDict[statsKey] : null);
                cell.GestureRecognizers.Add(tapGesture);

                HeatmapGrid.Add(cell, week, dayOfWeek);
                dayIndex++;
            }
        }

        // Update stats display
        UpdateStatsDisplay(allStats);
    }

    private void UpdateStatsDisplay(List<DailyStats> allStats)
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var todayStats = allStats.FirstOrDefault(s => s.Date == today);

        if (todayStats != null)
        {
            int totalItems = todayStats.TasksTotalCount + todayStats.HabitsTotalCount;
            int completedItems = todayStats.TasksDoneCount + todayStats.HabitsDoneCount;
            int percentage = totalItems > 0 ? (int)(todayStats.Intensity * 100) : 0;

            CompletedTodayLabel.Text = $"{percentage}%";
        }
        else
        {
            CompletedTodayLabel.Text = "0%";
        }

        // Calculate current streak (find last consecutive days with activity)
        int currentStreak = 0;
        var checkDate = DateTime.Today;
        var statsDict = allStats.ToDictionary(s => s.Date);

        while (true)
        {
            var dateKey = checkDate.ToString("yyyy-MM-dd");
            if (statsDict.ContainsKey(dateKey) && statsDict[dateKey].Intensity > 0)
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            else if (checkDate.Date == DateTime.Today.Date && currentStreak == 0)
            {
                // If today has no activity, start from yesterday
                checkDate = checkDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        CurrentStreakLabel.Text = $"{currentStreak} days";
    }

    private Color GetHeatmapColor(double intensity)
    {
        return intensity switch
        {
            0 => Color.FromHex("#0f172a"),        // No activity
            < 0.25 => Color.FromHex("#06b6d4"),   // Low (25%)
            < 0.5 => Color.FromHex("#0891b2"),    // Medium (50%)
            < 0.75 => Color.FromHex("#0e7490"),   // High (75%)
            _ => Color.FromHex("#065f73")         // Very High (100%)
        };
    }

    private async void OnCellTapped(DateTime date, DailyStats stats)
    {
        var message = stats != null
            ? $"{date:ddd, MMM dd}\n?? Tasks: {stats.TasksDoneCount}/{stats.TasksTotalCount}\n?? Habits: {stats.HabitsDoneCount}/{stats.HabitsTotalCount}\n?? {(stats.Intensity * 100):F0}% complete"
            : $"{date:ddd, MMM dd}\nNo data recorded";

        await Application.Current?.MainPage?.DisplayAlert("Daily Details", message, "OK");
    }
}

