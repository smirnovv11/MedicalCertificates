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
    /// Логика взаимодействия для AddCourse.xaml
    /// </summary>
    public partial class AddCourse : Window
    {
        MedCertificatesDbContext db;
        bool[] isValid;
        int maxCourseValue;

        public AddCourse()
        {
            try
            {
                InitializeComponent();
                db = new MedCertificatesDbContext();

                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                isValid = new bool[3] { false, true, true };
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


        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentcb.SelectedItem != null && departmentcb.SelectedIndex >= 0)
            {
                maxCourseValue = (departmentcb.SelectedItem as DepartmentsTable).MaxCourse;
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[0] = true;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void maxCoursetb_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text.Length > 0
                && Convert.ToInt32((sender as TextBox).Text) > 0
                && Convert.ToInt32((sender as TextBox).Text) <= maxCourseValue)
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[1] = true;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
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
                AddDepartmentToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить курс.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddDepartmentToDb()
        {
            var departmentId = new SqlParameter("@DepId", (departmentcb.SelectedItem as DepartmentsTable).DepartmentId);
            var number = new SqlParameter("@Number", numbertb.Text);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateCourse_procedure @DepId, @Number", departmentId, number);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.YesButton.Focus();
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
    }
}
