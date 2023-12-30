using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для AddStudent.xaml
    /// </summary>
    public partial class AddStudent : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public AddStudent()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();

                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                isValid = new bool[6] { false, false, false, false, false, false };
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
            int index = Convert.ToInt32((sender as TextBox).Name.Substring((sender as TextBox).Name.Length - 1, 1)) - 1;
            if ((sender as TextBox).Text.Length <= 0)
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[index] = false;
            }
            else
            {
                (sender as TextBox).BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[index] = true;
            }
        }

        private void birthDatedp_LostFocus(object sender, RoutedEventArgs e)
        {
            if (birthDatedp.SelectedDate >= DateTime.Now)
            {
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[3] = false;
            }
            else
            {
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[3] = true;
            }
        }

        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentcb.SelectedIndex != null)
            {
                groupcb.IsEnabled = true;
                groupcb.ItemsSource = db.GroupsTables
                    .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                groupcb.DisplayMemberPath = "Name";
            }
            else
                groupcb.IsEnabled = false;
        }

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentcb.SelectedItem == null)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[4] = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[4] = true;
            }
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupcb.SelectedItem == null)
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[5] = false;
            }
            else
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[5] = true;
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
                AddStudentToDb();

                var alert = new Alert("Добавление", "Учащийся добавлен.");
                alert.ShowDialog();
                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить учащегося.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddStudentToDb()
        {
            var groupId = new SqlParameter("@GroupId", (groupcb.SelectedItem as GroupsTable).GroupId);
            var firstName = new SqlParameter("@FirstName", firstNametb2.Text);
            var secondName = new SqlParameter("@SecondName", secondNametb1.Text);
            var thirdName = new SqlParameter("@ThirdName", thirdNametb3.Text);
            var birthDate = new SqlParameter("@BirthDate", birthDatedp.SelectedDate);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateStudent_procedure @GroupId, @FirstName, @SecondName, @ThirdName, @BirthDate", groupId, firstName, secondName, thirdName, birthDate);
        }
    }
}
