namespace LifeDashboard;

public class HabitItem
{
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int Streak { get; set; }

    public DateTime LastCompletedDate { get; set; }
}
