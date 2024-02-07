using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
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
    /// Логика взаимодействия для UpdateStudent.xaml
    /// </summary>
    public partial class UpdateStudent : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;
        int studentId;

        public UpdateStudent(int studentId)
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();
                this.studentId = studentId;

                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                var item = db.StudentsTables.First(s => s.StudentId == studentId);
                var group = db.GroupsTables.First(g => g.GroupId == item.GroupId);
                var course = db.CoursesTables.First(c => c.CourseId == group.CourseId);

                departmentcb.SelectedIndex = departmentcb.Items.IndexOf(db.DepartmentsTables.First(d => d.DepartmentId == course.DepartmentId));

                groupcb.ItemsSource = db.GroupsTables
                    .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                groupcb.DisplayMemberPath = "Name";
                groupcb.SelectedIndex = groupcb.Items.IndexOf(group);

                firstNametb2.Text = item.FirstName;
                secondNametb1.Text = item.SecondName;
                thirdNametb3.Text = item.ThirdName;

                birthDatedp.SelectedDate = item.BirthDate;

                isValid = new bool[6] { true, true, true, true, true, true };
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
            if (departmentcb.SelectedIndex != null && departmentcb.SelectedIndex >= 0)
            {
                groupcb.IsEnabled = true;
                groupcb.ItemsSource = db.GroupsTables
                    .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                groupcb.DisplayMemberPath = "Name";

                if (groupcb.Items.Count <= 0)
                    isValid[5] = false;
            }
            else
                groupcb.IsEnabled = false;
        }

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentcb.SelectedItem == null || departmentcb.SelectedIndex < 0)
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
            if (groupcb.SelectedItem == null || groupcb.SelectedIndex < 0)
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

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Изменение", $"Не удалось обновить учащегося.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddStudentToDb()
        {
            var id = new SqlParameter("@Id", studentId);
            var groupId = new SqlParameter("@GroupId", (groupcb.SelectedItem as GroupsTable).GroupId);
            var firstName = new SqlParameter("@FirstName", firstNametb2.Text);
            var secondName = new SqlParameter("@SecondName", secondNametb1.Text);
            var thirdName = new SqlParameter("@ThirdName", thirdNametb3.Text);
            var birthDate = new SqlParameter("@BirthDate", birthDatedp.SelectedDate);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC UpdateStudent_procedure @Id, @GroupId, @FirstName, @SecondName, @ThirdName, @BirthDate", id, groupId, firstName, secondName, thirdName, birthDate);
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
