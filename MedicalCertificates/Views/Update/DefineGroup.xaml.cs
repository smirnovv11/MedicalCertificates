using DocumentFormat.OpenXml.Vml.Spreadsheet;
using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для DefineGroup.xaml
    /// </summary>
    public partial class DefineGroup : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;
        IList students;

        public DefineGroup(IList collection)
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();
                students = collection;

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

        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentcb.SelectedIndex != null)
            {
                groupcb.IsEnabled = true;
                groupcb.ItemsSource = db.GroupsTables
                    .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                groupcb.DisplayMemberPath = "Name";

                if (groupcb.Items.Count <= 0)
                    isValid[1] = false;
            }
            else
                groupcb.IsEnabled = false;
        }

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentcb.SelectedItem == null)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[0] = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[0] = true;
            }
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupcb.SelectedItem == null)
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
            else
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
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
                AddStudentToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Изменение", $"Не удалось определить студентов.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddStudentToDb()
        {
            foreach (var el in students)
            {
                var id = new SqlParameter("@Id", (el as DataGridView).StudentId);
                var groupId = new SqlParameter("@GroupId", (groupcb.SelectedItem as GroupsTable).GroupId);

                db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC UpdateStudentGroup_procedure @Id, @GroupId", id, groupId);
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
