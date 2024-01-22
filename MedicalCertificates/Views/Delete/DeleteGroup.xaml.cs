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

namespace MedicalCertificates.Views.Delete
{
    /// <summary>
    /// Логика взаимодействия для DeleteGroup.xaml
    /// </summary>
    public partial class DeleteGroup : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public DeleteGroup()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();
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

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var el in isValid)
            {
                if (!el)
                {
                    var alert = new Alert("Ошибка!", "Не все поля заполены или введеные значения неверны.", AlertType.Error);
                    alert.ShowDialog();
                    return;
                }
            }

            try
            {
                this.PreviewKeyDown -= Window_PreviewKeyDown;
                var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить группу?\nВсе студенты состоящие в данной группе будут удалены.");
                if (alert.ShowDialog() == true)
                    DeleteDepartmentFromDb();
                else
                    this.PreviewKeyDown += Window_PreviewKeyDown;
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void DeleteDepartmentFromDb()
        {
            var id = new SqlParameter("@Id", (groupcb.SelectedItem as GroupsTable).GroupId);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteGroup_procedure @Id", id);
            this.DialogResult = true;
            Close();
        }

        private void departmentscb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentscb.SelectedIndex < 0)
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

        private void departmentscb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentscb.SelectedIndex >= 0)
            {
                groupcb.IsEnabled = true;
                groupcb.ItemsSource = db.GroupsTables
                    .Where(g => g.Course.Department.DepartmentId == (departmentscb.SelectedItem as DepartmentsTable).DepartmentId)
                    .OrderBy(c => c.Name)
                    .ToList();
                groupcb.DisplayMemberPath = "Name";
            }
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupcb.SelectedIndex < 0)
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
            else
            {
                groupcb.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[1] = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.YesButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
