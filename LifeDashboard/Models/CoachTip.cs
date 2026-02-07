namespace LifeDashboard.Models;

/// <summary>
/// Represents an AI-generated coaching tip.
/// </summary>
public class CoachTip
{
    /// <summary>
    /// The actionable tip (max 140 chars).
    /// </summary>
    public string Tip { get; set; }

    /// <summary>
    /// Motivational message.
    /// </summary>
    public string Motivation { get; set; }

    /// <summary>
    /// The source of the tip: "local" or "remote".
    /// </summary>
    public string Source { get; set; } = "local";
}
