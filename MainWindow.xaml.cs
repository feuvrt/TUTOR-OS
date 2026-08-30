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
        await LoadHomeworks();
        await LoadPayments();
        await LoadMaterials();
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
            PaymentStudentInput.ItemsSource = students;
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
        HomeworkLessonInput.ItemsSource = lessons;
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
private async Task LoadHomeworks()
{
    try
    {
        List<Homework> homeworks = new List<Homework>();

        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT
                h.id,
                h.lesson_id,
                s.name,
                h.deadline,
                h.content,
                h.status,
                h.teacher_comment
            FROM homeworks h
            JOIN lessons l
                ON l.id = h.lesson_id
            JOIN students s
                ON s.id = l.student_id
            ORDER BY h.deadline;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Homework homework = new Homework
            {
                Id = reader.GetInt32(0),

                LessonId = reader.GetInt32(1),

                StudentName = reader.GetString(2),

                Deadline = reader.IsDBNull(3)
                    ? null
                    : reader.GetDateTime(3),

                Content = reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4),

                Status = reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5),

                TeacherComment = reader.IsDBNull(6)
                    ? ""
                    : reader.GetString(6)
            };

            homeworks.Add(homework);
        }

        HomeworkTable.ItemsSource = homeworks;
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Домашние задания не загружены:\n" +
            ex.Message
        );
    }
}
private async void AddHomework_Click(
    object sender,
    RoutedEventArgs e)
{
    Lesson? selectedLesson =
        HomeworkLessonInput.SelectedItem as Lesson;

    if (selectedLesson == null)
    {
        MessageBox.Show("Выберите занятие");
        return;
    }

    if (HomeworkDeadlineInput.SelectedDate == null)
    {
        MessageBox.Show("Выберите дедлайн");
        return;
    }

    if (HomeworkContentInput.Text == "")
    {
        MessageBox.Show("Введите домашнее задание");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO homeworks
                (lesson_id, deadline, content, status)
            VALUES
                (@lessonId, @deadline, @content, @status);
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "lessonId",
            selectedLesson.Id
        );

        command.Parameters.AddWithValue(
            "deadline",
            HomeworkDeadlineInput.SelectedDate.Value
        );

        command.Parameters.AddWithValue(
            "content",
            HomeworkContentInput.Text
        );

        command.Parameters.AddWithValue(
            "status",
            "Задано"
        );

        await command.ExecuteNonQueryAsync();

        HomeworkDeadlineInput.SelectedDate = null;
        HomeworkContentInput.Clear();

        await LoadHomeworks();

        MessageBox.Show("Домашнее задание добавлено");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Домашнее задание не добавлено:\n" +
            ex.Message
        );
    }
}
private async void CompleteHomework_Click(
    object sender,
    RoutedEventArgs e)
{
    Homework? selectedHomework =
        HomeworkTable.SelectedItem as Homework;

    if (selectedHomework == null)
    {
        MessageBox.Show("Выберите домашнее задание");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            UPDATE homeworks
            SET status = 'Выполнено'
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedHomework.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadHomeworks();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Статус не изменён:\n" +
            ex.Message
        );
    }
}
private async void DeleteHomework_Click(
    object sender,
    RoutedEventArgs e)
{
    Homework? selectedHomework =
        HomeworkTable.SelectedItem as Homework;

    if (selectedHomework == null)
    {
        MessageBox.Show("Выберите домашнее задание");
        return;
    }

    MessageBoxResult result = MessageBox.Show(
        "Удалить домашнее задание?",
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
            DELETE FROM homeworks
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedHomework.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadHomeworks();

        MessageBox.Show("Домашнее задание удалено");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Домашнее задание не удалено:\n" +
            ex.Message
        );
    }
}
private async void CheckHomework_Click(
    object sender,
    RoutedEventArgs e)
{
    Homework? selectedHomework =
        HomeworkTable.SelectedItem as Homework;

    if (selectedHomework == null)
    {
        MessageBox.Show("Выберите домашнее задание");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            UPDATE homeworks
            SET status = 'Проверено'
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedHomework.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadHomeworks();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Статус не изменён:\n" +
            ex.Message
        );
    }
}

private async Task LoadPayments()
{
    try
    {
        List<Payment> payments = new List<Payment>();

        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT
                p.id,
                p.student_id,
                s.name,
                p.amount,
                p.payment_date,
                p.status,
                p.comment
            FROM payments p
            JOIN students s
                ON s.id = p.student_id
            ORDER BY p.payment_date DESC;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Payment payment = new Payment
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetInt32(1),
                StudentName = reader.GetString(2),
                Amount = reader.GetInt32(3),

                PaymentDate = reader.IsDBNull(4)
                    ? null
                    : reader.GetDateTime(4),

                Status = reader.IsDBNull(5)
                    ? ""
                    : reader.GetString(5),

                Comment = reader.IsDBNull(6)
                    ? ""
                    : reader.GetString(6)
            };

            payments.Add(payment);
        }

        PaymentsTable.ItemsSource = payments;
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Оплаты не загружены:\n" +
            ex.Message
        );
    }
}

private async void AddPayment_Click(
    object sender,
    RoutedEventArgs e)
{
    Student? selectedStudent =
        PaymentStudentInput.SelectedItem as Student;

    if (selectedStudent == null)
    {
        MessageBox.Show("Выберите ученика");
        return;
    }

    if (!int.TryParse(
        PaymentAmountInput.Text,
        out int amount))
    {
        MessageBox.Show("Сумма должна быть числом");
        return;
    }

    if (PaymentDateInput.SelectedDate == null)
    {
        MessageBox.Show("Выберите дату");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO payments
                (student_id, amount, payment_date, status, comment)
            VALUES
                (@studentId, @amount, @date, @status, @comment);
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "studentId",
            selectedStudent.Id
        );

        command.Parameters.AddWithValue(
            "amount",
            amount
        );

        command.Parameters.AddWithValue(
            "date",
            PaymentDateInput.SelectedDate.Value
        );

        command.Parameters.AddWithValue(
            "status",
            "Ожидается"
        );

        command.Parameters.AddWithValue(
            "comment",
            PaymentCommentInput.Text
        );

        await command.ExecuteNonQueryAsync();

        PaymentAmountInput.Clear();
        PaymentDateInput.SelectedDate = null;
        PaymentCommentInput.Clear();

        await LoadPayments();

        MessageBox.Show("Оплата добавлена");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Оплата не добавлена:\n" +
            ex.Message
        );
    }
}

private async void CompletePayment_Click(
    object sender,
    RoutedEventArgs e)
{
    Payment? selectedPayment =
        PaymentsTable.SelectedItem as Payment;

    if (selectedPayment == null)
    {
        MessageBox.Show("Выберите оплату");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            UPDATE payments
            SET status = 'Оплачено'
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedPayment.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadPayments();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Статус оплаты не изменён:\n" +
            ex.Message
        );
    }
}

private async void DeletePayment_Click(
    object sender,
    RoutedEventArgs e)
{
    Payment? selectedPayment =
        PaymentsTable.SelectedItem as Payment;

    if (selectedPayment == null)
    {
        MessageBox.Show("Выберите оплату");
        return;
    }

    MessageBoxResult result = MessageBox.Show(
        $"Удалить оплату {selectedPayment.Amount} руб.?",
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
            DELETE FROM payments
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedPayment.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadPayments();

        MessageBox.Show("Оплата удалена");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Оплата не удалена:\n" +
            ex.Message
        );
    }
}

private async Task LoadMaterials()
{
    try
    {
        List<Material> materials = new List<Material>();

        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            SELECT id, title, topic, link, comment
            FROM materials
            ORDER BY id DESC;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Material material = new Material
            {
                Id = reader.GetInt32(0),

                Title = reader.GetString(1),

                Topic = reader.IsDBNull(2)
                    ? ""
                    : reader.GetString(2),

                Link = reader.IsDBNull(3)
                    ? ""
                    : reader.GetString(3),

                Comment = reader.IsDBNull(4)
                    ? ""
                    : reader.GetString(4)
            };

            materials.Add(material);
        }

        MaterialsTable.ItemsSource = materials;
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Материалы не загружены:\n" +
            ex.Message
        );
    }
}

private async void AddMaterial_Click(
    object sender,
    RoutedEventArgs e)
{
    if (MaterialTitleInput.Text == "")
    {
        MessageBox.Show("Введите название материала");
        return;
    }

    try
    {
        await using var connection = Database.GetConnection();
        await connection.OpenAsync();

        string sql = @"
            INSERT INTO materials
                (title, topic, link, comment)
            VALUES
                (@title, @topic, @link, @comment);
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "title",
            MaterialTitleInput.Text
        );

        command.Parameters.AddWithValue(
            "topic",
            MaterialTopicInput.Text
        );

        command.Parameters.AddWithValue(
            "link",
            MaterialLinkInput.Text
        );

        command.Parameters.AddWithValue(
            "comment",
            MaterialCommentInput.Text
        );

        await command.ExecuteNonQueryAsync();

        MaterialTitleInput.Clear();
        MaterialTopicInput.Clear();
        MaterialLinkInput.Clear();
        MaterialCommentInput.Clear();

        await LoadMaterials();

        MessageBox.Show("Материал добавлен");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Материал не добавлен:\n" +
            ex.Message
        );
    }
}

private async void DeleteMaterial_Click(
    object sender,
    RoutedEventArgs e)
{
    Material? selectedMaterial =
        MaterialsTable.SelectedItem as Material;

    if (selectedMaterial == null)
    {
        MessageBox.Show("Выберите материал");
        return;
    }

    MessageBoxResult result = MessageBox.Show(
        $"Удалить материал «{selectedMaterial.Title}»?",
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
            DELETE FROM materials
            WHERE id = @id;
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id",
            selectedMaterial.Id
        );

        await command.ExecuteNonQueryAsync();

        await LoadMaterials();

        MessageBox.Show("Материал удалён");
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Материал не удалён:\n" +
            ex.Message
        );
    }
}

private async void CreateStudentAccount_Click(
    object sender,
    RoutedEventArgs e)
{
    Student? selectedStudent =
        StudentsTable.SelectedItem as Student;

    if (selectedStudent == null)
    {
        MessageBox.Show(
            "Сначала выберите ученика в таблице"
        );

        return;
    }

    if (StudentLoginInput.Text == "")
    {
        MessageBox.Show("Введите логин");
        return;
    }

    if (StudentPasswordInput.Password == "")
    {
        MessageBox.Show("Введите пароль");
        return;
    }

    try
    {
        await using var connection =
            Database.GetConnection();

        await connection.OpenAsync();



        string studentCheckSql = @"
            SELECT COUNT(*)
            FROM users
            WHERE student_id = @studentId;
        ";

        await using var studentCheckCommand =
            new NpgsqlCommand(
                studentCheckSql,
                connection
            );

        studentCheckCommand.Parameters.AddWithValue(
            "studentId",
            selectedStudent.Id
        );

        long studentAccountCount =
            (long)(await studentCheckCommand.ExecuteScalarAsync())!;

        if (studentAccountCount > 0)
        {
            MessageBox.Show(
                "У этого ученика уже есть аккаунт"
            );

            return;
        }


        string loginCheckSql = @"
            SELECT COUNT(*)
            FROM users
            WHERE login = @login;
        ";

        await using var loginCheckCommand =
            new NpgsqlCommand(
                loginCheckSql,
                connection
            );

        loginCheckCommand.Parameters.AddWithValue(
            "login",
            StudentLoginInput.Text
        );

        long loginCount =
            (long)(await loginCheckCommand.ExecuteScalarAsync())!;

        if (loginCount > 0)
        {
            MessageBox.Show(
                "Такой логин уже используется"
            );

            return;
        }


        string passwordHash =
            PasswordHelper.HashPassword(
                StudentPasswordInput.Password
            );


        string sql = @"
            INSERT INTO users
                (
                    login,
                    password_hash,
                    role,
                    student_id
                )
            VALUES
                (
                    @login,
                    @password,
                    'student',
                    @studentId
                );
        ";

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "login",
            StudentLoginInput.Text
        );

        command.Parameters.AddWithValue(
            "password",
            passwordHash
        );

        command.Parameters.AddWithValue(
            "studentId",
            selectedStudent.Id
        );

        await command.ExecuteNonQueryAsync();

        StudentLoginInput.Clear();
        StudentPasswordInput.Clear();
 
        MessageBox.Show(
            $"Аккаунт для {selectedStudent.Name} создан"
        );
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Аккаунт не создан:\n" +
            ex.Message
        );
    }
}


}