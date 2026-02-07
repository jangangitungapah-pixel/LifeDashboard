namespace LifeDashboard.Models;

/// <summary>
/// Records an XP gain event for audit/analytics purposes.
/// </summary>
public class XPEvent
{
    /// <summary>
    /// Source of XP: "task", "habit", "streak", "achievement".
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Amount of XP gained.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Timestamp of the XP gain.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Related task or habit ID (if applicable).
    /// </summary>
    public string RelatedId { get; set; }

    /// <summary>
    /// Additional context (e.g., streak count, habit title).
    /// </summary>
    public string Context { get; set; }
}
