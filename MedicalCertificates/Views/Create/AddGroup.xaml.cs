using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MedicalCertificates.Views.Create
{
    /// <summary>
    /// Логика взаимодействия для AddGroup.xaml
    /// </summary>
    public partial class AddGroup : Window
    {
        MedCertificatesDbContext db;
        bool[] isValid;
        int maxCourseValue;

        public AddGroup()
        {
            try
            {
                InitializeComponent();
                db = new MedCertificatesDbContext();

                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                isValid = new bool[3] { false, false, false };
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentcb.SelectedItem != null && departmentcb.SelectedIndex >= 0)
            {
                maxCourseValue = (departmentcb.SelectedItem as DepartmentsTable).MaxCourse;
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);

                coursecb.IsEnabled = true;
                coursecb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                coursecb.DisplayMemberPath = "Number";

                isValid[0] = true;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[0] = false;
            }
        }

        private void coursecb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (coursecb.SelectedItem != null && coursecb.SelectedIndex >= 0)
            {
                courseBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[1] = true;
            }
            else
            {
                courseBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
        }

        private void Nametb_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text.Length <= 0)
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[2] = false;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[2] = true;
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var el in isValid)
                if (!el)
                {
                    var alert = new Alert("Ошибка!", "Не все поля заполены или введеные значения неверны.", AlertType.Error);
                    alert.ShowDialog();
                    return;
                }

            try
            {
                AddDepartmentToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить группу.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddDepartmentToDb()
        {
            var courseId = new SqlParameter("@CourseId", (coursecb.SelectedItem as CoursesTable).CourseId);
            var name = new SqlParameter("@Name", nametb.Text);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateGroup_procedure @CourseId, @Name", courseId, name);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                YesButton.Focus();
                this.YesButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void addDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddDepartment();
            if (wind.ShowDialog() == true)
            {
                db = new MedCertificatesDbContext();
                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
            }
        }
        private void addCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddCourse();
            if (wind.ShowDialog() == true)
            {
                db = new MedCertificatesDbContext();
                if (departmentcb.SelectedIndex >= 0)
                {
                    coursecb.IsEnabled = true;
                    coursecb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                    coursecb.DisplayMemberPath = "Number";
                }
            }
        }
    }
}
