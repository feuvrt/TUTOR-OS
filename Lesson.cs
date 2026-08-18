namespace TutorOS;

public class Lesson
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = "";

    public DateTime? ScheduledAt { get; set; }

    public int DurationMinutes { get; set; }

    public string Topic { get; set; } = "";

    public string Status { get; set; } = "";

    public string DisplayName
{
    get
    {
        if (ScheduledAt.HasValue)
        {
            return $"{StudentName} — {ScheduledAt.Value:dd.MM HH:mm}";
        }

        return StudentName;
    }
}
}