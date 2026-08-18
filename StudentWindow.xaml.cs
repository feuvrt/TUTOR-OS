using System.Windows;

namespace TutorOS;

public partial class StudentWindow : Window
{
    private int studentId;

    public StudentWindow(int studentId)
    {
        InitializeComponent();

        this.studentId = studentId;
    }
}