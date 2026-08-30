using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;

namespace TutorOS;

public partial class StudentWindow : Window
{
    private int studentId;

    public StudentWindow(int studentId)
    {
        InitializeComponent();

        this.studentId = studentId;

        Loaded += StudentWindow_Loaded;
    }

    private async void StudentWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadStudent();
        await LoadLessons();
        await LoadHomeworks();
    }

    private async Task LoadStudent()
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT name
            FROM students
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            studentId
        );

        object? result =
            await command.ExecuteScalarAsync();

        if (result != null)
        {
            StudentNameText.Text =
                $"Личный кабинет — {result}";
        }
    }

    private async Task LoadLessons()
    {
        List<Lesson> lessons = new List<Lesson>();

        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT
                id,
                student_id,
                scheduled_at,
                duration_minutes,
                topic,
                status
            FROM lessons
            WHERE student_id = @studentId
            ORDER BY scheduled_at;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "studentId",
            studentId
        );

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Lesson lesson = new Lesson
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),

                ScheduledAt = reader.IsDBNull(2)
                    ? null
                    : reader.GetDateTime(2),

                DurationMinutes = reader.IsDBNull(3)
                    ? 60
                    : reader.GetInt32(3),

                Topic = reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4),

                Status = reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5)
            };

            lessons.Add(lesson);
        }

        StudentLessonsTable.ItemsSource = lessons;
    }

    private async Task LoadHomeworks()
    {
        List<Homework> homeworks =
            new List<Homework>();

        await using var connection =
            Database.GetConnection();

        await connection.OpenAsync();

        string sql = @"
            SELECT
                h.id,
                h.lesson_id,
                h.deadline,
                h.content,
                h.status,
                h.teacher_comment
            FROM homeworks h
            JOIN lessons l
                ON l.id = h.lesson_id
            WHERE l.student_id = @studentId
            ORDER BY h.deadline;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "studentId",
            studentId
        );

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Homework homework =
                new Homework
                {
                    Id = reader.GetInt32(0),
                    LessonId = reader.GetInt32(1),

                    Deadline = reader.IsDBNull(2)
                        ? null
                        : reader.GetDateTime(2),

                    Content = reader.IsDBNull(3)
                        ? ""
                        : reader.GetString(3),

                    Status = reader.IsDBNull(4)
                        ? ""
                        : reader.GetString(4),

                    TeacherComment = reader.IsDBNull(5)
                        ? ""
                        : reader.GetString(5)
                };

            homeworks.Add(homework);
        }

        StudentHomeworkTable.ItemsSource = homeworks;
    }
    private async void CompleteHomework_Click(
    object sender,
    RoutedEventArgs e)
{
    Homework? selectedHomework =
        StudentHomeworkTable.SelectedItem as Homework;

    if (selectedHomework == null)
    {
        MessageBox.Show(
            "Выберите домашнее задание"
        );
        return;
    }

    await using var connection =
        Database.GetConnection();

    await connection.OpenAsync();

    string sql = @"
        UPDATE homeworks
        SET status = 'Выполнено'
        WHERE id = @homeworkId
        AND lesson_id IN
        (
            SELECT id
            FROM lessons
            WHERE student_id = @studentId
        );
    ";

    await using var command =
        new NpgsqlCommand(sql, connection);

    command.Parameters.AddWithValue(
        "homeworkId",
        selectedHomework.Id
    );

    command.Parameters.AddWithValue(
        "studentId",
        studentId
    );

    await command.ExecuteNonQueryAsync();

    await LoadHomeworks();
}
}