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
    /// Логика взаимодействия для UpdateDepartment.xaml
    /// </summary>
    public partial class UpdateDepartment : Window
    {
        MedCertificatesDbContext db;
        bool[] isValid;

        public UpdateDepartment()
        {
            try
            {
                InitializeComponent();
                db = new MedCertificatesDbContext();
                departmentscb.ItemsSource = db.DepartmentsTables.ToList();
                departmentscb.DisplayMemberPath = "Name";

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

        private void Nametb_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text.Length <= 0)
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[0] = false;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[0] = true;
            }
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
                isValid[1] = true;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
        }

        private void departmentscb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentscb.SelectedIndex < 0)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
        }

        private void departmentscb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentscb.SelectedIndex >= 0)
            {
                var item = (departmentscb.SelectedItem as DepartmentsTable);
                nametb.Text = item.Name;
                maxCoursetb.Text = item.MaxCourse.ToString();
                nametb.IsEnabled = true;
                maxCoursetb.IsEnabled = true;

                isValid[0] = true;
                isValid[1] = true;
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
                UpdateDepartmentToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Изменение", $"Не удалось обновить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void UpdateDepartmentToDb()
        {
            var id = new SqlParameter("@Id", (departmentscb.SelectedItem as DepartmentsTable).DepartmentId);
            var name = new SqlParameter("@Name", nametb.Text);
            var maxCourse = new SqlParameter("@MaxCourse", maxCoursetb.Text);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC UpdateDepartment_procedure @Id, @Name, @MaxCourse", id, name, maxCourse);
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
