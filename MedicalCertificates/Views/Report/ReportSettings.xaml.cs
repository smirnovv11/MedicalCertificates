using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
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

namespace MedicalCertificates.Views.Report
{
    /// <summary>
    /// Логика взаимодействия для ReportSettings.xaml
    /// </summary>
    public partial class ReportSettings : Window
    {
        private bool[] isValid;
        MedicalCertificatesDbContext db;

        public ReportSettings(ReportType type)
        {
            InitializeComponent();

            db = new MedicalCertificatesDbContext();
            departmentcb.ItemsSource = db.DepartmentsTables.ToList();
            departmentcb.DisplayMemberPath = "Name";
            isValid = new bool[5] { true, true, false, true, true };
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

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentcb.SelectedItem == null || departmentcb.SelectedIndex < 0)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[2] = false;

                courseCb.IsEnabled = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[2] = true;

                courseCb.IsEnabled = true;
                courseCb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId)
                    .ToList();
                courseCb.DisplayMemberPath = "Number";
            }
        }

        private void coursecb_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (groupcb.SelectedItem == null || groupcb.SelectedIndex < 0)
            //{
            //    groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
            //    isValid[5] = false;
            //}
            //else
            //{
            //    groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            //    isValid[5] = true;
            //}
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (groupcb.SelectedItem == null || groupcb.SelectedIndex < 0)
            //{
            //    groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
            //    isValid[5] = false;
            //}
            //else
            //{
            //    groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            //    isValid[5] = true;
            //}
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            Close();
        }

        private void endDateCb_Checked_1(object sender, RoutedEventArgs e)
        {
            birthDatedp.IsEnabled = true;
            isValid[1] = false;
        }

        private void endDateCb_Unchecked(object sender, RoutedEventArgs e)
        {
            birthDatedp.IsEnabled = false;
            birthDatedp.SelectedDate = null;
            isValid[1] = true;
        }

        private void birthDatedp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (birthDatedp.SelectedDate > DateTime.Now)
            {
                isValid[1] = true;
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                isValid[1] = false;
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
