using System.Text.Json;
using System.Timers;
using LifeDashboard.Data;
using LifeDashboard.Models;
using LifeDashboard.Services;
using LifeDashboard.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LifeDashboard;

public partial class MainPage : ContentPage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private System.Timers.Timer? _timer;

    private List<TaskItem> _tasks = new();
    private List<HabitItem> _habits = new();
    private List<DailyStats> _dailyStats = new();

    private string _taskPath =>
        Path.Combine(FileSystem.AppDataDirectory, "tasks.json");

    private string _habitPath =>
        Path.Combine(FileSystem.AppDataDirectory, "habits.json");

    private bool _achievementShownToday;
    private bool _pageReady;
    private bool _isInitialized;

    // Services
    private IXPService _xpService = null!;
    private IAchievementService _achievementService = null!;
    private IAIService _aiService = null!;
    private INotificationService _notificationService = null!;
    private PersistenceHelper _persistence = null!;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            InitializeServices();

            // Initialize async services
            await _xpService.InitializeAsync();
            await _notificationService.InitializeAsync();

            // Load data
            LoadTasks();
            LoadHabits();
            _dailyStats = _persistence.LoadDailyStats();

            // Setup event handlers
            _xpService.LevelUp += OnLevelUp;

            _isInitialized = true;
        }

        // Initial setup
        DailyReset();
        StartClock();
        RefreshTasks();
        RefreshHabits();
        UpdateStats();
        UpdateGamificationUI();
        UpdateHeatmap();

        // Load initial coach tip
        _ = LoadCoachTip();

        // Mark page as ready for popups
        _pageReady = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopClock();
        _pageReady = false;
    }

    private void InitializeServices()
    {
        var services = IPlatformApplication.Current.Services;
        _persistence = services.GetRequiredService<PersistenceHelper>();
        _xpService = services.GetRequiredService<IXPService>();
        _achievementService = services.GetRequiredService<IAchievementService>();
        _aiService = services.GetRequiredService<IAIService>();
        _notificationService = services.GetRequiredService<INotificationService>();
    }

    // ================== GAMIFICATION ==================

    private async void OnLevelUp(object sender, LevelUpEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!_pageReady) return;

            // Show level up popup
            var badge = _achievementService.GetAllBadges()
                .FirstOrDefault(b => _xpService.Profile.Badges.Contains(b.Id));

            await LevelUpPopup.ShowAsync(e.NewLevel, e.TotalXp, badge);

            // Play confetti
            OverlayGrid.IsVisible = true;
            await ConfettiView.PlayAsync();
            OverlayGrid.IsVisible = false;

            UpdateGamificationUI();
        });
    }




    private async void UpdateGamificationUI()
    {
        LevelLabel.Text = $"Level {_xpService.Level}";

        // Calculate XP for current level
        long currentLevelXp = _xpService.GetXpRequiredForLevel(_xpService.Level);
        long nextLevelXp = _xpService.GetXpRequiredForLevel(_xpService.Level + 1);
        long xpInLevel = _xpService.TotalXp - currentLevelXp;
        long xpNeeded = nextLevelXp - currentLevelXp;

        XpLabel.Text = $"{xpInLevel} / {xpNeeded} XP";

        // Calculate and display percentage
        double progress = _xpService.GetProgressToNextLevel();
        int percentage = (int)(progress * 100);
        XpPercentLabel.Text = $"{percentage}%";

        await XpProgressBar.ProgressTo(progress, 400, Easing.CubicOut);

        // Update badges
        BadgesContainer.Children.Clear();
        foreach (var badgeId in _xpService.Profile.Badges)
        {
            var badge = _achievementService.GetAllBadges().FirstOrDefault(b => b.Id == badgeId);
            if (badge != null)
            {
                var badgeFrame = new Frame
                {
                    BackgroundColor = Color.FromHex("#1e293b"),
                    BorderColor = Color.FromHex("#06b6d4"),
                    CornerRadius = 12,
                    Padding = 10,
                    WidthRequest = 50,
                    HeightRequest = 50,
                    HasShadow = true
                };

                var label = new Label
                {
                    Text = badge.Icon,
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                badgeFrame.Content = label;

                // Tap to show description
                var gesture = new TapGestureRecognizer();
                gesture.Tapped += async (s, e) => await ShowBadgeInfo(badge);
                badgeFrame.GestureRecognizers.Add(gesture);

                BadgesContainer.Add(badgeFrame);
            }
        }
    }


    private async Task ShowBadgeInfo(Badge badge)
    {
        await DisplayAlert(
            $"{badge.Icon} {badge.Name}",
            badge.Description,
            "Got it!"
        );
    }

    private async Task LoadCoachTip()
    {
        try
        {
            var context = $"Tasks: {_tasks.Count}, Habits: {_habits.Count}, Level: {_xpService.Level}";
            var tip = await _aiService.GetTipAsync(context);
            CoachTipLabel.Text = $"{tip.Tip}\n\n✨ {tip.Motivation}";
        }
        catch
        {
            CoachTipLabel.Text = "Failed to load tip. Try again!";
        }
    }

    private async void OnGetCoachTip(object sender, EventArgs e)
    {
        CoachTipLabel.Text = "Loading...";
        await LoadCoachTip();
    }

    private void UpdateHeatmap()
    {
        HeatmapView.Render(_dailyStats);
    }



    // ================== CLOCK ================

    private void StartClock()
    {
        if (_timer != null)
        {
            _timer.Start();
            return;
        }

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DateLabel.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            });
        };
        _timer.Start();
    }

    private void StopClock()
    {
        if (_timer == null)
            return;

        _timer.Stop();
        _timer.Dispose();
        _timer = null;
    }

    // ================== DAILY RESET ================

    private void DailyReset()
    {
        DateTime today = DateTime.Today;

        foreach (var habit in _habits)
        {
            if (habit.LastCompletedDate.Date < today)
                habit.IsCompleted = false;
        }

        _achievementShownToday = false;
        SaveHabits();
        UpdateDailyStats();
    }

    // ================== TASKS ================

    private void OnAddTask(object sender, EventArgs e)
    {
        var title = TaskEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
        {
            _tasks.Add(new TaskItem
            {
                Title = title,
                IsCompleted = false
            });

            TaskEntry.Text = string.Empty;
            SaveTasks();
            RefreshTasks();
        }
    }

    private void OnDeleteTask(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem &&
            swipeItem.BindingContext is TaskItem task)
        {
            _tasks.Remove(task);
            SaveTasks();
            RefreshTasks();
        }
    }

    private void OnDeleteTaskButton(object sender, EventArgs e)
    {
        if (sender is Button btn &&
            btn.BindingContext is TaskItem task)
        {
            _tasks.Remove(task);
            SaveTasks();
            RefreshTasks();
        }
    }

    private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        SaveTasks();
        UpdateStats();

        // Award XP when task completed
        if (e.Value)
        {
            await _xpService.AddXpAsync(10, "task");
        }
    }

    private void SaveTasks()
    {
        try
        {
            var json = JsonSerializer.Serialize(_tasks, JsonOptions);
            File.WriteAllText(_taskPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving tasks: {ex.Message}");
        }
    }

    private void LoadTasks()
    {
        try
        {
            if (!File.Exists(_taskPath))
                return;

            var json = File.ReadAllText(_taskPath);
            _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json)
                     ?? new List<TaskItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
            _tasks = new List<TaskItem>();
        }
    }

    private void RefreshTasks()
    {
        TaskList.ItemsSource = null;
        TaskList.ItemsSource = _tasks;
        UpdateStats();
    }

    // ================== HABITS ================

    private void OnAddHabit(object sender, EventArgs e)
    {
        var title = HabitEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
        {
            _habits.Add(new HabitItem
            {
                Title = title,
                IsCompleted = false,
                Streak = 0,
                LastCompletedDate = DateTime.MinValue
            });

            HabitEntry.Text = string.Empty;
            SaveHabits();
            RefreshHabits();
        }
    }

    private async void OnHabitCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox box &&
            box.BindingContext is HabitItem habit)
        {
            DateTime today = DateTime.Today;

            if (e.Value)
            {
                if (habit.LastCompletedDate.Date == today.AddDays(-1))
                    habit.Streak++;
                else if (habit.LastCompletedDate.Date != today)
                    habit.Streak = 1;

                habit.LastCompletedDate = today;

                // Award XP for habit
                await _xpService.AddXpAsync(5, "habit");

                // Check streak badges
                await _achievementService.CheckStreakBadgesAsync(habit.Streak, _xpService.Profile);

                // Award streak bonus XP
                if (habit.Streak == 3)
                    await _xpService.AddXpAsync(10, "streak", null, "3-day streak");
                else if (habit.Streak == 7)
                    await _xpService.AddXpAsync(30, "streak", null, "7-day streak");
                else if (habit.Streak == 14)
                    await _xpService.AddXpAsync(70, "streak", null, "14-day streak");
                else if (habit.Streak == 30)
                    await _xpService.AddXpAsync(200, "streak", null, "30-day streak");
            }

            SaveHabits();
            UpdateStats();
            UpdateGamificationUI();
        }
    }

    private void SaveHabits()
    {
        try
        {
            var json = JsonSerializer.Serialize(_habits, JsonOptions);
            File.WriteAllText(_habitPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving habits: {ex.Message}");
        }
    }

    private void LoadHabits()
    {
        try
        {
            if (!File.Exists(_habitPath))
                return;

            var json = File.ReadAllText(_habitPath);
            _habits = JsonSerializer.Deserialize<List<HabitItem>>(json)
                      ?? new List<HabitItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading habits: {ex.Message}");
            _habits = new List<HabitItem>();
        }
    }

    private void RefreshHabits()
    {
        HabitList.ItemsSource = null;
        HabitList.ItemsSource = _habits;
        UpdateStats();
    }

    // ================== STATS ================

    private async void UpdateStats()
    {
        int taskDone = _tasks.Count(t => t.IsCompleted);
        int habitDone = _habits.Count(h => h.IsCompleted);

        TaskStatsLabel.Text = $"{taskDone}/{_tasks.Count}";
        HabitStatsLabel.Text = $"{habitDone}/{_habits.Count}";

        double taskProgress =
            _tasks.Count == 0 ? 0 :
            (double)taskDone / _tasks.Count;

        double habitProgress =
            _habits.Count == 0 ? 0 :
            (double)habitDone / _habits.Count;

        await TaskProgressBar.ProgressTo(taskProgress, 400, Easing.CubicOut);
        await HabitProgressBar.ProgressTo(habitProgress, 400, Easing.CubicOut);

        CheckAchievement(taskDone, habitDone);
        UpdateDailyStats();
    }

    private void UpdateDailyStats()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var todayStats = _dailyStats.FirstOrDefault(s => s.Date == today);

        int taskDone = _tasks.Count(t => t.IsCompleted);
        int habitDone = _habits.Count(h => h.IsCompleted);

        if (todayStats == null)
        {
            todayStats = new DailyStats { Date = today };
            _dailyStats.Add(todayStats);
        }

        todayStats.TasksDoneCount = taskDone;
        todayStats.TasksTotalCount = _tasks.Count;
        todayStats.HabitsDoneCount = habitDone;
        todayStats.HabitsTotalCount = _habits.Count;

        // Compute intensity (0.0 - 1.0)
        int totalItems = _tasks.Count + _habits.Count;
        if (totalItems > 0)
        {
            todayStats.Intensity = (double)(taskDone + habitDone) / totalItems;
        }
        else
        {
            todayStats.Intensity = 0;
        }

        _persistence.SaveDailyStats(_dailyStats);
        UpdateHeatmap();
    }

    private async void CheckAchievement(int taskDone, int habitDone)
    {
        if (_achievementShownToday || !_pageReady) return;

        if (_tasks.Count > 0 &&
            _habits.Count > 0 &&
            taskDone == _tasks.Count &&
            habitDone == _habits.Count)
        {
            _achievementShownToday = true;

            await DisplayAlert(
                "🎉 Achievement Unlocked!",
                "You completed EVERYTHING today!\nLegend mode activated 😄🔥",
                "Nice!"
            );

            // Play confetti
            OverlayGrid.IsVisible = true;
            await ConfettiView.PlayAsync();
            OverlayGrid.IsVisible = false;
        }
    }
}



