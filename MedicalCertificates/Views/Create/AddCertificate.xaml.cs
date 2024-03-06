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

namespace MedicalCertificates.Views.Create
{
    /// <summary>
    /// Логика взаимодействия для AddCertificate.xaml
    /// </summary>
    public partial class AddCertificate : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public AddCertificate()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();

                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";

                healthGroupcb.ItemsSource = db.HealthGroupTables.ToList();
                healthGroupcb.DisplayMemberPath = "HealthGroup";

                PEGroupcb.ItemsSource = db.PegroupTables.ToList();
                PEGroupcb.DisplayMemberPath = "Pegroup";

                isValid = new bool[7] { false, false, false, false, false, false, false };
                
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }
        public AddCertificate(GroupsTable group) : this()
        {
            groupcb.IsEnabled = true;
            studentcb.IsEnabled = true;

            var currGroup = db.GroupsTables.First(g => g.GroupId == group.GroupId);
            var dep = db.DepartmentsTables.First(d => d.DepartmentId
            == db.CoursesTables.First(c => c.CourseId == currGroup.CourseId).DepartmentId);

            departmentcb.SelectedIndex = departmentcb.Items.IndexOf(dep);
            isValid[0] = true;

            groupcb.SelectedIndex = groupcb.Items.IndexOf(currGroup);
            isValid[1] = true;
        }
        public AddCertificate(StudentsTable student) : this()
        {
            groupcb.IsEnabled = true;
            studentcb.IsEnabled = true;

            var group = db.GroupsTables.First(g => g.GroupId == student.GroupId);
            var dep = db.DepartmentsTables.First(d => d.DepartmentId
            == db.CoursesTables.First(c => c.CourseId == group.CourseId).DepartmentId);
            var studentDb = db.StudentsTables.First(s => s.StudentId == student.StudentId);

            departmentcb.SelectedIndex = departmentcb.Items.IndexOf(dep);
            isValid[0] = true;

            groupcb.SelectedIndex = groupcb.Items.IndexOf(group);
            isValid[1] = true;

            studentcb.SelectedIndex = studentcb.Items.IndexOf(student.SecondName + " " + student.FirstName + " " + student.ThirdName);
            isValid[2] = true;
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
            if (departmentcb.SelectedIndex != null && departmentcb.SelectedIndex >= 0)
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
            if (departmentcb.SelectedItem == null || departmentcb.SelectedIndex < 0)
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

        private void groupcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (groupcb.SelectedIndex != null && groupcb.SelectedIndex >= 0)
            {
                studentcb.IsEnabled = true;
                studentcb.ItemsSource = db.StudentsTables
                    .Where(s => s.GroupId == (groupcb.SelectedItem as GroupsTable).GroupId)
                    .OrderBy(s => s.SecondName)
                    .Select(p => $"{p.SecondName} {p.FirstName} {p.ThirdName}").ToList();
            }
            else
                studentcb.IsEnabled = false;
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupcb.SelectedItem == null || groupcb.SelectedIndex < 0)
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

        private void studentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (studentcb.SelectedItem == null || studentcb.SelectedIndex < 0)
            {
                studentcb.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[2] = false;
            }
            else
            {
                studentcb.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[2] = true;
            }
        }

        private void healthGroupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (healthGroupcb.SelectedItem == null || healthGroupcb.SelectedIndex < 0)
            {
                healthGroupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[3] = false;
            }
            else
            {
                healthGroupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[3] = true;
            }
        }

        private void PEGroupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PEGroupcb.SelectedItem == null || PEGroupcb.SelectedIndex < 0)
            {
                PEGroupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[4] = false;
            }
            else
            {
                PEGroupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[4] = true;
            }
        }

        private void issueDatedp_LostFocus(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate == null || validDatedp.SelectedDate > issueDatedp.SelectedDate)
            {
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[5] = false;
            }
            else
            {
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[5] = true;
            }
        }

        private void validDatedp_LostFocus(object sender, RoutedEventArgs e)
        {
            if (validDatedp.SelectedDate == null || validDatedp.SelectedDate <= issueDatedp.SelectedDate)
            {
                validDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[6] = false;
            }
            else
            {
                validDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[6] = true;
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
                var alert = new Alert("Добавление", $"Не удалось добавить справку.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddStudentToDb()
        {
            var name = studentcb.SelectedItem.ToString().Split(' ');
            var studentId = new SqlParameter("@StudentId", db.StudentsTables.FirstOrDefault(s => s.FirstName == name[1] && s.SecondName == name[0] && s.ThirdName == name[2]).StudentId);
            var healthId = new SqlParameter("@Health", (healthGroupcb.SelectedItem as HealthGroupTable).HealthGroupId);
            var peId = new SqlParameter("@PE", (PEGroupcb.SelectedItem as PegroupTable).PegroupId);
            var issueDate = new SqlParameter("@Issue", issueDatedp.SelectedDate);
            var validDate = new SqlParameter("@Valid", validDatedp.SelectedDate);
            var note = new SqlParameter("@Note", notetb.Text);
            
            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateCertificate_procedure @StudentId, @Health, @PE, @Issue, @Valid, @Note", studentId, healthId, peId, issueDate, validDate, note);
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
                db = new MedicalCertificatesDbContext();
                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
            }
        }

        private void addGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddGroup();
            if (wind.ShowDialog() == true)
            {
                db = new MedicalCertificatesDbContext();
                if (departmentcb.SelectedIndex >= 0)
                {
                    groupcb.IsEnabled = true;
                    groupcb.ItemsSource = db.GroupsTables
                        .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
                    groupcb.DisplayMemberPath = "Name";
                }
            }
        }

        private void addStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddStudent();
            if (wind.ShowDialog() == true)
            {
                db = new MedicalCertificatesDbContext();
                if (groupcb.SelectedIndex >= 0)
                {
                    studentcb.IsEnabled = true;
                    studentcb.ItemsSource = db.StudentsTables
                        .Where(s => s.GroupId == (groupcb.SelectedItem as GroupsTable).GroupId)
                        .OrderBy(s => s.SecondName)
                        .Select(p => $"{p.SecondName} {p.FirstName} {p.ThirdName}").ToList();
                }
            }
        }

        private void clearValidDateButton_Click(object sender, RoutedEventArgs e)
        {
            validDatedp.SelectedDate = null;
            isValid[6] = false;
        }

        private void addYear_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(12);
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[5] = true;
            }
            isValid[6] = true;
        }

        private void add6Month_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(6);
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[5] = true;
            }
            isValid[6] = true;
        }

        private void add3Month_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(3);
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[5] = true;
            }
            isValid[6] = true;
        }
    }
}
