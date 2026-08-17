using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using Npgsql;

namespace TutorOS;

public partial class MainWindow : Window
{

    private Student? editingStudent;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadStudents();
        await LoadLessons();
    }

    private async Task LoadStudents()
    {
        try
        {
            List<Student> students = new List<Student>();

            await using var connection = Database.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT id, name, exam, price, phone
                FROM students
                ORDER BY id;
            ";

            await using var command = new NpgsqlCommand(sql, connection);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Student student = new Student
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Exam = reader.GetString(2),
                    Price = reader.GetInt32(3),
                    Phone = reader.IsDBNull(4)
                        ? ""
                        : reader.GetString(4)
                };

                students.Add(student);
            }

            StudentsTable.ItemsSource = students;
            LessonStudentInput.ItemsSource = students;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ученики не загружены:\n" + ex.Message
            );
        }
    }
    private async void AddStudent_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (NameInput.Text == "")
        {
            MessageBox.Show("Введите имя ученика");
            return;
        }

        if (ExamInput.Text == "")
        {
            MessageBox.Show("Введите экзамен");
            return;
        }

        if (!int.TryParse(PriceInput.Text, out int price))
        {
            MessageBox.Show("Цена должна быть числом");
            return;
        }

        try
        {
            await using var connection = Database.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO students
                    (name, exam, price, phone)
                VALUES
                    (@name, @exam, @price, @phone);
            ";

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "name",
                NameInput.Text
            );

            command.Parameters.AddWithValue(
                "exam",
                ExamInput.Text
            );

            command.Parameters.AddWithValue(
                "price",
                price
            );

            command.Parameters.AddWithValue(
                "phone",
                PhoneInput.Text
            );

            await command.ExecuteNonQueryAsync();

            NameInput.Clear();
            ExamInput.Clear();
            PriceInput.Clear();
            PhoneInput.Clear();

            await LoadStudents();

            MessageBox.Show("Ученик добавлен");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ученик не добавлен:\n" + ex.Message
            );
        }
    }


    private async void DeleteStudent_Click(
    object sender,
    RoutedEventArgs e)
{
    Student? selectedStudent =
        StudentsTable.SelectedItem as Student;

    if (selectedStudent == null)
    {
        MessageBox.Show("Выберите ученика");
        return;
    }

    MessageBoxResult result = MessageBox.Show(
        $"Удалить ученика {selectedStudent.Name}?",
        "Удаление",
        MessageBoxButton.YesNo
    );

    if (result != MessageBoxResult.Yes)
    {
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            DELETE FROM students
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedStudent.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadStudents();

        MessageBox.Show("Ученик удалён");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Ученик не удалён:\n" +
            ex.Message
        );
    }

    
}
private void EditStudent_Click(
    object sender,
    RoutedEventArgs e)
{
    Student? selectedStudent =
        StudentsTable.SelectedItem as Student;

    if (selectedStudent == null)
    {
        MessageBox.Show("Выберите ученика");
        return;
    }

    editingStudent = selectedStudent;

    NameInput.Text = selectedStudent.Name;
    ExamInput.Text = selectedStudent.Exam;
    PriceInput.Text = selectedStudent.Price.ToString();
    PhoneInput.Text = selectedStudent.Phone;
}

private async void SaveStudent_Click(
    object sender,
    RoutedEventArgs e)
{
    if (editingStudent == null)
    {
        MessageBox.Show("Сначала выберите ученика для редактирования");
        return;
    }

    if (NameInput.Text == "")
    {
        MessageBox.Show("Введите имя ученика");
        return;
    }

    if (ExamInput.Text == "")
    {
        MessageBox.Show("Введите экзамен");
        return;
    }

    if (!int.TryParse(PriceInput.Text, out int price))
    {
        MessageBox.Show("Цена должна быть числом");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            UPDATE students
            SET name = @name,
                exam = @exam,
                price = @price,
                phone = @phone
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "name",
            NameInput.Text
        );

        command.Parameters.AddWithValue(
            "exam",
            ExamInput.Text
        );

        command.Parameters.AddWithValue(
            "price",
            price
        );

        command.Parameters.AddWithValue(
            "phone",
            PhoneInput.Text
        );

        command.Parameters.AddWithValue(
            "id",
            editingStudent.Id
        );

        await command.ExecuteNonQueryAsync();

        editingStudent = null;

        NameInput.Clear();
        ExamInput.Clear();
        PriceInput.Clear();
        PhoneInput.Clear();

        await LoadStudents();

        MessageBox.Show("Данные ученика изменены");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Изменения не сохранены:\n" +
            ex.Message
        );
    }
}

    private void HideAllSections()
{
    StudentsSection.Visibility = Visibility.Collapsed;
    ScheduleSection.Visibility = Visibility.Collapsed;
    HomeworkSection.Visibility = Visibility.Collapsed;
    PaymentsSection.Visibility = Visibility.Collapsed;
    MaterialsSection.Visibility = Visibility.Collapsed;
}

private void StudentsButton_Click(object sender, RoutedEventArgs e)
{
    HideAllSections();

    StudentsSection.Visibility = Visibility.Visible;
}

private void ScheduleButton_Click(object sender, RoutedEventArgs e)
{
    HideAllSections();

    ScheduleSection.Visibility = Visibility.Visible;
}

private void HomeworkButton_Click(object sender, RoutedEventArgs e)
{
    HideAllSections();

    HomeworkSection.Visibility = Visibility.Visible;
}

private void PaymentsButton_Click(object sender, RoutedEventArgs e)
{
    HideAllSections();

    PaymentsSection.Visibility = Visibility.Visible;
}

private void MaterialsButton_Click(object sender, RoutedEventArgs e)
{
    HideAllSections();

    MaterialsSection.Visibility = Visibility.Visible;
}

private async Task LoadLessons()
{
    try
    {
        List<Lesson> lessons = new List<Lesson>();

        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT
                l.id,
                l.student_id,
                s.name,
                l.scheduled_at,
                l.duration_minutes,
                l.topic,
                l.status
            FROM lessons l
            JOIN students s
                ON s.id = l.student_id
            ORDER BY l.scheduled_at;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Lesson lesson = new Lesson
            {
                Id = reader.GetInt32(0),

                StudentId = reader.GetInt32(1),

                StudentName = reader.GetString(2),

                ScheduledAt = reader.IsDBNull(3)
                    ? null
                    : reader.GetDateTime(3),

                DurationMinutes = reader.IsDBNull(4)
                    ? 60
                    : reader.GetInt32(4),

                Topic = reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5),

                Status = reader.IsDBNull(6)
                    ? ""
                    : reader.GetString(6)
            };

            lessons.Add(lesson);
        }

        LessonsTable.ItemsSource = lessons;
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Расписание не загружено:\n" +
            ex.Message
        );
    }
}

private async void AddLesson_Click(
    object sender,
    RoutedEventArgs e)
{
    Student? selectedStudent =
        LessonStudentInput.SelectedItem as Student;

    if (selectedStudent == null)
    {
        MessageBox.Show("Выберите ученика");
        return;
    }

    if (LessonDateInput.SelectedDate == null)
    {
        MessageBox.Show("Выберите дату");
        return;
    }

    if (!TimeSpan.TryParse(
        LessonTimeInput.Text,
        out TimeSpan time))
    {
        MessageBox.Show("Введите время, например 18:00");
        return;
    }

    if (!int.TryParse(
        LessonDurationInput.Text,
        out int duration))
    {
        MessageBox.Show("Длительность должна быть числом");
        return;
    }

    DateTime scheduledAt =
        LessonDateInput.SelectedDate.Value.Date + time;

    try
    {
        await using var connection =
            Database.GetConnection();

        await connection.OpenAsync();

        string sql = @"
            INSERT INTO lessons
                (
                    student_id,
                    scheduled_at,
                    duration_minutes,
                    topic,
                    status
                )
            VALUES
                (
                    @studentId,
                    @scheduledAt,
                    @duration,
                    @topic,
                    @status
                );
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "studentId",
            selectedStudent.Id
        );

        command.Parameters.AddWithValue(
            "scheduledAt",
            scheduledAt
        );

        command.Parameters.AddWithValue(
            "duration",
            duration
        );

        command.Parameters.AddWithValue(
            "topic",
            LessonTopicInput.Text
        );

        command.Parameters.AddWithValue(
            "status",
            "Запланирован"
        );

        await command.ExecuteNonQueryAsync();

        LessonDateInput.SelectedDate = null;
        LessonTimeInput.Text = "18:00";
        LessonTopicInput.Clear();
        LessonDurationInput.Text = "60";

        await LoadLessons();

        MessageBox.Show("Занятие добавлено");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Занятие не добавлено:\n" +
            ex.Message
        );
    }
}
private async void DeleteLesson_Click(
    object sender,
    RoutedEventArgs e)
{
    Lesson? selectedLesson =
        LessonsTable.SelectedItem as Lesson;

    if (selectedLesson == null)
    {
        MessageBox.Show("Выберите занятие");
        return;
    }

    MessageBoxResult result = MessageBox.Show(
        $"Удалить занятие с {selectedLesson.StudentName}?",
        "Удаление",
        MessageBoxButton.YesNo
    );

    if (result != MessageBoxResult.Yes)
    {
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            DELETE FROM lessons
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedLesson.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadLessons();

        MessageBox.Show("Занятие удалено");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Занятие не удалено:\n" +
            ex.Message
        );
    }
}
private async void CompleteLesson_Click(
    object sender,
    RoutedEventArgs e)
{
    Lesson? selectedLesson =
        LessonsTable.SelectedItem as Lesson;

    if (selectedLesson == null)
    {
        MessageBox.Show("Выберите занятие");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            UPDATE lessons
            SET status = 'Проведено'
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedLesson.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadLessons();

        MessageBox.Show("Занятие отмечено как проведённое");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Статус не изменён:\n" +
            ex.Message
        );
    }
}
}