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

namespace MedicalCertificates.Views.Update
{
    /// <summary>
    /// Логика взаимодействия для UpdateCourse.xaml
    /// </summary>
    public partial class UpdateCourse : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public UpdateCourse()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();
                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                isValid = new bool[2] { false, false };
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void maxCoursetb_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text.Length > 0
                && Convert.ToInt32((sender as TextBox).Text) > 0
                && Convert.ToInt32((sender as TextBox).Text) <= 5)
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[0] = true;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[0] = false;
            }
        }

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentcb.SelectedIndex < 0)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
        }

        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentcb.SelectedIndex >= 0)
            {
                coursecb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                coursecb.DisplayMemberPath = "Number";
                coursecb.IsEnabled = true;

                numbertb.Text = "";

                isValid[0] = false;
                isValid[1] = false;
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
                UpdateCourseToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Изменение", $"Не удалось обновить курс.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void UpdateCourseToDb()
        {
            var id = new SqlParameter("@Id", (coursecb.SelectedItem as CoursesTable).CourseId);
            var name = new SqlParameter("@Number", numbertb.Text);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC UpdateCourse_procedure @Id, @Number", id, name);
        }

        private void coursecb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (coursecb.SelectedIndex < 0)
            {
                courseBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                courseBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
        }

        private void coursecb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (coursecb.SelectedIndex >= 0)
            {
                var item = coursecb.SelectedItem as CoursesTable;
                numbertb.Text = item.Number.ToString();

                numbertb.IsEnabled = true;

                isValid[0] = true;
                isValid[1] = true;
            }
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
    }
}
