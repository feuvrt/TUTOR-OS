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
}