namespace LifeDashboard.Models;

/// <summary>
/// Represents the user's profile including XP, level, and achievements.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Total accumulated experience points.
    /// </summary>
    public long TotalXp { get; set; }

    /// <summary>
    /// Current user level computed from total XP.
    /// Level = floor(sqrt(TotalXp / 100))
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// List of earned badge IDs (e.g., "streak_3", "streak_7", "streak_14", "streak_30").
    /// </summary>
    public List<string> Badges { get; set; } = new();

    /// <summary>
    /// Date of the last level up.
    /// </summary>
    public DateTime LastLevelUpDate { get; set; }

    /// <summary>
    /// Last login date (for tracking daily streaks globally).
    /// </summary>
    public DateTime LastLoginDate { get; set; }
}
