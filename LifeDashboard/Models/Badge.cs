namespace LifeDashboard.Models;

/// <summary>
/// Represents a badge/achievement that can be earned.
/// </summary>
public class Badge
{
    /// <summary>
    /// Unique identifier for the badge (e.g., "streak_3").
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the badge.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Unicode emoji or icon representation.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Description explaining how to earn this badge.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Requirement to earn the badge (e.g., streak count of 3).
    /// </summary>
    public int Requirement { get; set; }

    /// <summary>
    /// Type of badge: "streak", "level", "achievement".
    /// </summary>
    public string Type { get; set; }
}
