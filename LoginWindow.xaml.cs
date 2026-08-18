using System;
using System.Windows;
using Npgsql;

namespace TutorOS;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }


    private async void CreateTeacher_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (LoginInput.Text == "")
        {
            MessageBox.Show("Введите логин");
            return;
        }

        if (PasswordInput.Password == "")
        {
            MessageBox.Show("Введите пароль");
            return;
        }

        try
        {
            await using var connection =
                Database.GetConnection();

            await connection.OpenAsync();

            string checkSql = @"
                SELECT COUNT(*)
                FROM users
                WHERE role = 'teacher';
            ";

            await using var checkCommand =
                new NpgsqlCommand(checkSql, connection);

            long count =
                (long)(await checkCommand.ExecuteScalarAsync())!;

            if (count > 0)
            {
                MessageBox.Show(
                    "Аккаунт преподавателя уже существует"
                );

                return;
            }

            string passwordHash =
                PasswordHelper.HashPassword(
                    PasswordInput.Password
                );

            string sql = @"
                INSERT INTO users
                    (login, password_hash, role)
                VALUES
                    (@login, @password, 'teacher');
            ";

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "login",
                LoginInput.Text
            );

            command.Parameters.AddWithValue(
                "password",
                passwordHash
            );

            await command.ExecuteNonQueryAsync();

            MessageBox.Show(
                "Аккаунт преподавателя создан"
            );

            PasswordInput.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Аккаунт не создан:\n" +
                ex.Message
            );
        }
    }


    private async void Login_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (LoginInput.Text == "" ||
            PasswordInput.Password == "")
        {
            MessageBox.Show("Введите логин и пароль");
            return;
        }

        try
        {
            await using var connection =
                Database.GetConnection();

            await connection.OpenAsync();

            string sql = @"
                SELECT
                    id,
                    login,
                    password_hash,
                    role,
                    student_id
                FROM users
                WHERE login = @login;
            ";

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "login",
                LoginInput.Text
            );

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                MessageBox.Show(
                    "Неверный логин или пароль"
                );

                return;
            }

            UserAccount account =
                new UserAccount
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),

                    StudentId = reader.IsDBNull(4)
                        ? null
                        : reader.GetInt32(4)
                };

            bool passwordCorrect =
                PasswordHelper.VerifyPassword(
                    PasswordInput.Password,
                    account.PasswordHash
                );

            if (!passwordCorrect)
            {
                MessageBox.Show(
                    "Неверный логин или пароль"
                );

                return;
            }

            if (account.Role == "teacher")
            {
                MainWindow mainWindow =
                    new MainWindow();

                mainWindow.Show();
                Close();
            }
            else if (
                account.Role == "student" &&
                account.StudentId != null)
            {
                StudentWindow studentWindow =
                    new StudentWindow(
                        account.StudentId.Value
                    );

                studentWindow.Show();
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ошибка входа:\n" +
                ex.Message
            );
        }
    }
}