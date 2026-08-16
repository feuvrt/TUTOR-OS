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
        try
        {
            await using var connection = Database.GetConnection();

            await connection.OpenAsync();

            MessageBox.Show("Успешое подключение к базе данных");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Ошибка подключения:\n" + ex.Message
            );
        }
    }
}