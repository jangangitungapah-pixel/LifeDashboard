namespace LifeDashboard.Views;

public partial class ConfettiView : ContentView
{
    /// <summary>
    /// Displays a confetti animation effect.
    /// Creates 30 falling squares with random colors, sizes, and rotation.
    /// </summary>
    public ConfettiView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Plays the confetti animation.
    /// </summary>
    public async Task PlayAsync()
    {
        ConfettiGrid.Children.Clear();

        var colors = new[] { Color.FromHex("#06b6d4"), Color.FromHex("#8b5cf6"), Color.FromHex("#ec4899"), Color.FromHex("#f59e0b") };
        var random = Random.Shared;

        // Get screen dimensions
        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        for (int i = 0; i < 30; i++)
        {
            var piece = new Frame
            {
                CornerRadius = random.Next(2, 6),
                BorderColor = Colors.Transparent,
                BackgroundColor = colors[random.Next(colors.Length)],
                WidthRequest = random.Next(6, 12),
                HeightRequest = random.Next(6, 12),
                Padding = 0,
                HasShadow = false
            };

            var randomX = random.Next(0, (int)screenWidth);
            var randomDuration = random.Next(800, 1500);
            var randomDelay = random.Next(0, 200);

            ConfettiGrid.Add(piece, 0, 0);

            _ = AnimateConfettiPieceAsync(piece, randomX, randomDuration, randomDelay, screenHeight);
        }

        await Task.Delay(2000);
        ConfettiGrid.Children.Clear();
    }

    private async Task AnimateConfettiPieceAsync(View piece, int targetX, int duration, int delay, double screenHeight)
    {
        await Task.Delay(delay);

        var startY = 0d;
        var endY = screenHeight;
        var random = Random.Shared;
        var rotation = random.Next(0, 360);

        piece.TranslationX = targetX;
        piece.TranslationY = startY;
        piece.Rotation = rotation;

        var tasks = new List<Task>
        {
            piece.TranslateTo(targetX + random.Next(-50, 50), endY, (uint)duration, Easing.Linear),
            piece.RotateTo(rotation + 360, (uint)duration, Easing.Linear)
        };

        await Task.WhenAll(tasks);
    }
}

