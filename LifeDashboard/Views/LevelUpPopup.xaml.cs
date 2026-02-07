using LifeDashboard.Models;

namespace LifeDashboard.Views;

public partial class LevelUpPopup : ContentView
{
    public event EventHandler Closed;

    /// <summary>
    /// Modal popup displayed when user levels up.
    /// Includes confetti animation and optional badge info.
    /// </summary>
    public LevelUpPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Shows the level up popup with animation.
    /// </summary>
    public async Task ShowAsync(int newLevel, long totalXp, Badge earnedBadge = null)
    {
        LevelLabel.Text = $"Level {newLevel}";
        XpLabel.Text = $"Total XP: {totalXp}";

        if (earnedBadge != null)
        {
            BadgeLabel.Text = $"{earnedBadge.Icon} {earnedBadge.Name}";
        }
        else
        {
            BadgeLabel.IsVisible = false;
        }

        PopupFrame.IsVisible = true;
        PopupFrame.Scale = 0.5;
        PopupFrame.Opacity = 0;

        await PopupFrame.ScaleTo(1.0, 400, Easing.SpringOut);
        await PopupFrame.FadeTo(1.0, 300);
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await PopupFrame.FadeTo(0, 300);
        PopupFrame.IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
