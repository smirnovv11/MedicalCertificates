﻿using MedicalCertificates.Models;
using MedicalCertificates.Services;
using MedicalCertificates.Services.Alert;
using MedicalCertificates.Views.Create;
using MedicalCertificates.Views.Delete;
using MedicalCertificates.Views.Update;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MedicalCertificates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MedicalCertificatesDbContext db;

        public MainWindow()
        {
            try
            {
                db = new MedicalCertificatesDbContext();
                InitializeComponent();

                CultureInfo cultureInfo = new CultureInfo("ru-RU");
                DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
                dateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                cultureInfo.DateTimeFormat = dateTimeFormat;
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                UpdateAllDbData();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight - taskbarHeight;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ReportMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender).ContextMenu;
            contextMenu.PlacementTarget = sender as UIElement;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            //DataGridSorting.HandleDataGridSorting((DataGrid)sender, e);
        }

        private void UpdateAllDbData()
        {
            db = new MedicalCertificatesDbContext();
            TreeMenu.ItemsSource = db.DepartmentsTables
                          .Include(d => d.CoursesTables.OrderBy(c => c.Number))
                          .ThenInclude(c => c.GroupsTables)
                          .ThenInclude(g => g.StudentsTables)
                          .ToList();

            var year = new SqlParameter("@Year", "2023");
            var group = new SqlParameter("@GroupId", "1");

            var res = db.DataGridViews.FromSqlRaw("SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure @Year, @GroupId", year, group).ToList();
            dataGrid.ItemsSource = res;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateAllDbData();
        }

        private void ShowNote_Click(object sender, RoutedEventArgs e)
        {
            var text = (dataGrid.SelectedItem as DataGridView).Note;

            if (text == null || text.Length <= 0)
                text = "Примечание отсутствует.";

            var alert = new Alert("Примечание", text);
            alert.ShowDialog();
        }


        #region Addition

        private void StudentAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddStudent();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }
        private void DepartmentAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddDepartment();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }
        private void CourseAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddCourse();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }
        private void GroupAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddGroup();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }
        private void CertificateAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddCertificate();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        #endregion

        private void DeleteStudentButton_Click(object sender, RoutedEventArgs e)
        {
            var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить выделенного студента?\nДанные о справках студента таже будут удалены.");
            if (dataGrid.SelectedIndex > -1 && alert.ShowDialog() == true)
            {
                var studentId = new SqlParameter("@StudentId", (dataGrid.SelectedItem as DataGridView).StudentId);

                db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteStudent_procedure @StudentId", studentId);
                UpdateAllDbData();
            }
        }

        private void DeleteCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить справку выделенного студента?");
            if (dataGrid.SelectedIndex > -1 && alert.ShowDialog() == true)
            {
                var certificateId = new SqlParameter("@Certificate", (dataGrid.SelectedItem as DataGridView).CertificateId);

                db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteCertificate_procedure @Certificate", certificateId);
                UpdateAllDbData();
            }
        }

        private void DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteDepartment();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteCourse();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteGroup();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void GroupTree_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateDepartment_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateDepartment();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void UpdateCourse_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateCourse();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void UpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateGroup();
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void UpdateStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex >= 0)
            {
                var wind = new UpdateStudent((dataGrid.SelectedItem as DataGridView).StudentId);
                if (wind.ShowDialog() == true)
                    UpdateAllDbData();
            }
        }

        private void UpdateCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex >= 0)
            {
                var wind = new UpdateCertificate((dataGrid.SelectedItem as DataGridView).CertificateId);
                if (wind.ShowDialog() == true)
                    UpdateAllDbData();
            }
        }

        private void RowDoubleClock_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateCertificate((dataGrid.SelectedItem as DataGridView).CertificateId);
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }
    }
}
