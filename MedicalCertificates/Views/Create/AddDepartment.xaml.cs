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
    /// Логика взаимодействия для AddDepartment.xaml
    /// </summary>
    public partial class AddDepartment : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public AddDepartment()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();

                isValid = new bool[2] { false, true };
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
                var alert = new Alert("Добавление", $"Не удалось добавить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddDepartmentToDb()
        {
            var name = new SqlParameter("@Name", nametb.Text);
            var maxCourse = new SqlParameter("@MaxCourse", maxCoursetb.Text);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateDepartment_procedure @Name, @MaxCourse", name, maxCourse);
        }
    }
}
