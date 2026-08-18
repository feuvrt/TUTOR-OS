namespace TutorOS;

public class Homework
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string StudentName { get; set; } = "";

    public DateTime? Deadline { get; set; }

    public string Content { get; set; } = "";

    public string Status { get; set; } = "";

    public string TeacherComment { get; set; } = "";
}