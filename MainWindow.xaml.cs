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
}