namespace LifeDashboard.Models;

/// <summary>
/// Records daily completion statistics for the heatmap.
/// </summary>
public class DailyStats
{
    /// <summary>
    /// Date in YYYY-MM-DD format.
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// Number of tasks completed on this date.
    /// </summary>
    public int TasksDoneCount { get; set; }

    /// <summary>
    /// Total number of tasks available on this date.
    /// </summary>
    public int TasksTotalCount { get; set; }

    /// <summary>
    /// Number of habits completed on this date.
    /// </summary>
    public int HabitsDoneCount { get; set; }

    /// <summary>
    /// Total number of habits available on this date.
    /// </summary>
    public int HabitsTotalCount { get; set; }

    /// <summary>
    /// Computed intensity 0.0-1.0 based on completion percentage.
    /// </summary>
    public double Intensity { get; set; }
}
