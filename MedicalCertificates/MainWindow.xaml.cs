using MedicalCertificates.Models;
using MedicalCertificates.Services;
using MedicalCertificates.Services.Alert;
using MedicalCertificates.Views.Create;
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
            TreeMenu.ItemsSource = db.DepartmentsTables
                          .Include(d => d.CoursesTables)
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

    }
}
