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

        private void departmentcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (departmentcb.SelectedIndex != null && departmentcb.SelectedIndex >= 0)
            //{
            //    groupcb.IsEnabled = true;
            //    groupcb.ItemsSource = db.GroupsTables
            //        .Where(g => g.Course.Department.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId).ToList();
            //    groupcb.DisplayMemberPath = "Name";
            //}
            //else
            //    groupcb.IsEnabled = false;
        }

        private void departmentcb_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (departmentcb.SelectedItem == null || departmentcb.SelectedIndex < 0)
            //{
            //    departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
            //    isValid[4] = false;
            //}
            //else
            //{
            //    departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            //    isValid[4] = true;
            //}
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
            
        }
    }
}
