using MedicalCertificates.Models;
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
using Microsoft.Win32;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace MedicalCertificates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MedicalCertificatesDbContext db;
        int currGroupId;
        int currStudentId;
        int currYear;

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

                UpdateTreeData();
                currStudentId = -1;

                TableLabel.Text = "Выберите группу или учащегося для просмотра информации";
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
            UpdateTreeData();
            UpdateGridData();
        }

        private void UpdateTreeData()
        {
            if (currGroupId != null && currGroupId >= 0)
            {
                db = new MedicalCertificatesDbContext();
                TreeMenu.ItemsSource = db.DepartmentsTables
                              .Include(d => d.CoursesTables.OrderBy(c => c.Number))
                              .ThenInclude(c => c.GroupsTables)
                              .ThenInclude(g => g.StudentsTables.OrderBy(s => s.SecondName).ThenBy(s => s.FirstName))
                              .ToList();
            }
        }

        private void UpdateGridData()
        {
            List<DataGridView>? res = new List<DataGridView>();
            if (currGroupId != null && currGroupId >= 0)
            {
                db = new MedicalCertificatesDbContext();
                var year = new SqlParameter("@Year", currYear);
                var group = new SqlParameter("@GroupId", currGroupId);

                res = db.DataGridViews.FromSqlRaw("SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure @Year, @GroupId", year, group).ToList();
            }
            else if (currStudentId != null && currStudentId >= 0)
            {
                db = new MedicalCertificatesDbContext();

                res = db.DataGridViews.Where(s => s.StudentId == currStudentId).ToList();
            }
            var unSortGroup = db.GroupsTables.First(g => g.CourseId
                                == db.CoursesTables.First(c => c.DepartmentId
                                == db.DepartmentsTables.First(d => d.Name == "Неопределенные")
                                .DepartmentId)
                                .CourseId)
                                .GroupId;

            if (currGroupId == unSortGroup)
            {
                dataGrid.SelectionMode = DataGridSelectionMode.Extended;
                UnSortGridControlPanel.Visibility = Visibility.Visible;
                DefaultControlPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                dataGrid.SelectionMode = DataGridSelectionMode.Single;
                UnSortGridControlPanel.Visibility = Visibility.Collapsed;
                DefaultControlPanel.Visibility = Visibility.Visible;
            }

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

        private void GroupTree_Click(object sender, RoutedEventArgs e)
        {
            var group = ((sender as Button).DataContext as GroupsTable);
            currGroupId = group.GroupId;
            currStudentId = -1;
            currYear = (int)group.Course.Year;

            UpdateYearCb();

            YearCb.Visibility = Visibility.Visible;
            UpdateGridData();

            TableLabel.Text = $"Листок здоровья группы {group.Name} ({group.Course.Number} курс)";
        }

        private void UpdateYearCb()
        {
            var course = db.CoursesTables.First(c => c.CourseId
            == db.GroupsTables.First(g => g.GroupId == currGroupId)
            .CourseId);

            List<string> years = new List<string>();
            int year = currYear;

            for (int i = course.Number; i >= 1; i--, year--)
            {
                years.Add(year.ToString().Substring(2, 2) + "/" + (year + 1).ToString().Substring(2, 2));
            }

            YearCb.ItemsSource = years;
            YearCb.SelectedIndex = 0;
        }

        private void YearCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currYear = currYear - YearCb.SelectedIndex;
            UpdateGridData();
        }

        private void StudentTree_Click(object sender, RoutedEventArgs e)
        {
            var student = ((sender as Button).DataContext as StudentsTable);
            currStudentId = student.StudentId;
            currGroupId = -1;

            YearCb.Visibility = Visibility.Hidden;
            UpdateGridData();

            TableLabel.Text = $"Листок здоровья учащегося ({student.SecondName + " " + student.FirstName + " " + student.ThirdName})";
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
                UpdateGridData();
        }

        #endregion

        #region Deleting

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

        private void DeleteStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить выделенного(-ых) студента(-ов)?\nДанные о справках студента(-ов) таже будут удалены.");
            if (dataGrid.SelectedIndex > -1 && alert.ShowDialog() == true)
            {
                foreach(var el in dataGrid.SelectedItems)
                {
                    var studentId = new SqlParameter("@StudentId", (el as DataGridView).StudentId);

                    db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteStudent_procedure @StudentId", studentId);
                }
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
                UpdateGridData();
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

        #endregion

        #region Updating

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
                CastUpdateWind();
            }
        }

        private void RowDoubleClock_Click(object sender, RoutedEventArgs e)
        {
            CastUpdateWind();
        }

        private void CastUpdateWind()
        {
            var wind = new UpdateCertificate((dataGrid.SelectedItem as DataGridView).CertificateId);
            wind.ShowDialog();
            if (wind.DialogResult == true && wind.toStudent == true)
            {
                var student = db.StudentsTables.First(s => s.StudentId == (dataGrid.SelectedItem as DataGridView).StudentId);
                currStudentId = student.StudentId;
                currGroupId = -1;

                YearCb.Visibility = Visibility.Hidden;
                UpdateGridData();

                TableLabel.Text = $"Листок здоровья учащегося ({student.SecondName + " " + student.FirstName + " " + student.ThirdName})";
            }
            else if (wind.DialogResult == true)
                UpdateGridData();
        }
        private void StudentTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var wind = new UpdateStudent(currStudentId);
            if (wind.ShowDialog() == true)
                UpdateAllDbData();
        }

        private void DefineGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex > -1)
            {
                var wind = new DefineGroup(dataGrid.SelectedItems);
                if (wind.ShowDialog() == true)
                    UpdateAllDbData();
            }
        }

        #endregion


        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Excel Files (*.xls;*.xlsx;*.xlsm)|*.xls;*.xlsx;*.xlsm";
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    using (var workbook = new XLWorkbook(dlg.FileName))
                    {
                        var worksheet = workbook.Worksheets.Worksheet(1);
                        var rowCount = worksheet.LastRowUsed().RowNumber();
                        var studentId = db.StudentsTables.OrderByDescending(s => s.StudentId).FirstOrDefault().StudentId + 1;
                        var group = db.GroupsTables.First(g => g.CourseId
                                == db.CoursesTables.First(c => c.DepartmentId
                                == db.DepartmentsTables.First(d => d.Name == "Неопределенные")
                                .DepartmentId)
                                .CourseId)
                                .GroupId;

                        for (int row = 1; row <= rowCount; ++row, studentId++)
                        {
                            var groupId = new SqlParameter("@GroupId", group);
                            var firstName = new SqlParameter("@FirstName", worksheet.Cell(row, 2).GetValue<String>());
                            var secondName = new SqlParameter("@SecondName", worksheet.Cell(row, 1).GetValue<String>());
                            var thirdName = new SqlParameter("@ThirdName", worksheet.Cell(row, 3).GetValue<String>());
                            var birthDate = new SqlParameter("@BirthDate", DateTime.Parse(worksheet.Cell(row, 4).GetValue<String>()));

                            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateStudent_procedure @GroupId, @FirstName, @SecondName, @ThirdName, @BirthDate", groupId, firstName, secondName, thirdName, birthDate);

                            var studentParam = new SqlParameter("@StudentId", studentId);
                            var healthId = new SqlParameter("@Health", 1);
                            var peId = new SqlParameter("@PE", 1);
                            var issueDate = new SqlParameter("@Issue", DateTime.Parse(worksheet.Cell(row, 4).GetValue<String>()));
                            var validDate = new SqlParameter("@Valid", DateTime.Parse(worksheet.Cell(row, 4).GetValue<String>()).AddMonths(1));
                            var note = new SqlParameter("@Note", "");

                            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateCertificate_procedure @StudentId, @Health, @PE, @Issue, @Valid, @Note", studentParam, healthId, peId, issueDate, validDate, note);
                        }

                        currGroupId = group;
                        currStudentId = -1;

                        YearCb.Visibility = Visibility.Hidden;
                        UpdateGridData();

                        TableLabel.Text = $"Листок здоровья неопределенных по группам учащихся";
                    }
                }
            }
            catch (Exception ex) 
            {
                var alert = new Alert("Ошибка!", ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

    }
}
