using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
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
        private bool isValid;
        MedCertificatesDbContext db;
        ReportType type;

        public ReportSettings(ReportType type)
        {
            InitializeComponent();

            try
            {
                this.type = type;
                switch (type)
                {
                    case ReportType.Group:
                    case ReportType.ShortGroupPE:
                        break;
                    case ReportType.Course:
                        groupPanel.Visibility = Visibility.Collapsed;
                        break;
                    case ReportType.Department:
                        groupPanel.Visibility = Visibility.Collapsed;
                        coursePanel.Visibility = Visibility.Collapsed;
                        break;
                    case ReportType.TotalReport:
                        groupGb.Visibility = Visibility.Collapsed;
                        this.Width = 400;
                        this.Height = 350;
                        groupPanel.Visibility = Visibility.Collapsed;
                        coursePanel.Visibility = Visibility.Collapsed;
                        departmentPanel.Visibility = Visibility.Collapsed;
                        break;
                }

                db = new MedCertificatesDbContext();
                if (type != ReportType.TotalReport)
                {
                    departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                    departmentcb.DisplayMemberPath = "Name";
                }
                isValid = true;
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message, AlertType.Error);
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
                departmentBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);

                courseCb.SelectedItem = null;
                courseCb.IsEnabled = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Gray);

                courseCb.IsEnabled = true;
                courseCb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentcb.SelectedItem as DepartmentsTable).DepartmentId)
                    .ToList();
                courseCb.DisplayMemberPath = "Number";
            }
        }

        private void coursecb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (courseCb.SelectedItem == null || courseCb.SelectedIndex < 0)
            {
                courseCb.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);

                groupcb.SelectedItem = null;
                groupcb.IsEnabled = false;
            }
            else
            {
                courseCb.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Gray);

                groupcb.IsEnabled = true;
                groupcb.ItemsSource = db.GroupsTables.Where(c => c.CourseId == (courseCb.SelectedItem as CoursesTable).CourseId)
                    .ToList();
                groupcb.DisplayMemberPath = "Name";
            }
        }

        private void groupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (groupcb.SelectedItem == null || groupcb.SelectedIndex < 0)
            {
                groupBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {
                groupBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid)
            {
                var alert = new Alert("Ошибка!", "Дата не была выбрана.", AlertType.Error);
                alert.ShowDialog();
                return;
            }

            if (cb1.IsChecked == false && cb3.IsChecked == false ||
                (type == ReportType.Department && departmentcb.SelectedIndex < 0) ||
                (type == ReportType.Course && courseCb.SelectedIndex < 0) ||
                (type == ReportType.Group && groupcb.SelectedIndex < 0)
                )
            {
                var alert = new Alert("Ошибка!", "Не все поля заполены или введеные значения неверны.", AlertType.Error);
                alert.ShowDialog();
                return;
            }

            var title = $"Отчет по всем отделениям";
            var res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudents_procedure").ToList();
            switch (type)
            {
                case ReportType.Department:
                    title = $"Отчет по отделению {(departmentcb.SelectedItem as DepartmentsTable).Name}";
                    break;
                case ReportType.Course:
                    title = $"Отчет по курсу {(courseCb.SelectedItem as CoursesTable).Number}, отделение {(departmentcb.SelectedItem as DepartmentsTable).Name}";
                    res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsCourse_procedure {(courseCb.SelectedItem as CoursesTable).CourseId}").ToList();
                    break;
                case ReportType.ShortGroupPE:
                case ReportType.Group:
                    title = $"Отчет по группе {(groupcb.SelectedItem as GroupsTable).Name}";
                    res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure {(groupcb.SelectedItem as GroupsTable).GroupId}").ToList();
                    break;
            }

            if (cb1.IsChecked == true && cb3.IsChecked == false)
            {
                res = res.Where(d => d.ValidDate.Date > DateTime.Now.Date).ToList();
            } 
            else if (cb1.IsChecked == false && cb3.IsChecked == true)
            {
                res = res.Where(d => d.ValidDate <= DateTime.Now).ToList();
            }

            if (birthDatedp.SelectedDate != null)
            {
                res = res.Where(d => d.ValidDate <= birthDatedp.SelectedDate.Value).ToList();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Сохранить файл Excel";
            string filePath = "";
            if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                filePath = saveFileDialog.FileName;
                if (type == ReportType.ShortGroupPE)
                {
                    title = $"Листок здоровья группы {(groupcb.SelectedItem as GroupsTable).Name}";
                    ExportToExcelShort(res, filePath, title);
                }
                else
                {
                    ExportToExcel(res, filePath, title);
                }
            }
            else
                return;

            var sucAlert = new Alert("Успешно", title + " был успешно создан.");
            sucAlert.ShowDialog();

            //Открываем файл в Excel
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

            Close();
        }

        private void ExportToExcel(List<DataGridView> data, string filePath, string title)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Лист1");


                    worksheet.Cell(1, 1).Value = title;
                    worksheet.Row(1).Height = 45;
                    worksheet.Range(1, 1, 1, 6).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.FontSize = 14;

                    worksheet.Cell(3, 1).Value = "ФИО Учащегося";
                    worksheet.Cell(3, 2).Value = "Дата рождения";
                    worksheet.Cell(3, 3).Value = "Группа здоровья";
                    worksheet.Cell(3, 4).Value = "Группа по физкультуре";
                    worksheet.Cell(3, 5).Value = "Справка открыта";
                    worksheet.Cell(3, 6).Value = "Справка годна";

                    worksheet.Range(3, 1, 3, 6).Style.Font.SetBold();
                    worksheet.Range(3, 1, 3, 6).Style.Font.SetItalic();

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 4, 1).Value = data[i].SecondName + " " + data[i].FirstName[0] + ". " + data[i].ThirdName[0] + ".";
                        worksheet.Cell(i + 4, 2).Value = data[i].BirthDate.ToString("dd.MM.yyyy");
                        worksheet.Cell(i + 4, 3).Value = data[i].HealthGroup;
                        worksheet.Cell(i + 4, 4).Value = data[i].Pegroup;
                        worksheet.Cell(i + 4, 5).Value = data[i].IssueDate.ToString("dd.MM.yyyy");
                        worksheet.Cell(i + 4, 6).Value = data[i].ValidDate.ToString("dd.MM.yyyy");

                    }

                    worksheet.Column(1).Width = 26;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 23;
                    worksheet.Column(5).Width = 19;
                    worksheet.Column(6).Width = 16;

                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message);
                alert.ShowDialog();
            }
        }

        private void ExportToExcelShort(List<DataGridView> data, string filePath, string title)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");


                    worksheet.Cell(1, 1).Value = title;
                    worksheet.Range(1, 1, 1, 3).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Font.SetBold()
                        .Font.FontSize = 14;

                    worksheet.Cell(3, 1).Value = "ФИО";
                    worksheet.Cell(3, 2).Value = "Группа здоровья";
                    worksheet.Cell(3, 3).Value = "Группа по физкультуре";

                    worksheet.Range(3, 1, 3, 3).Style.Font.SetBold();
                    worksheet.Range(3, 1, 3, 3).Style.Font.SetItalic();

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 4, 1).Value = $"{data[i].SecondName} {data[i].FirstName[0]}. {data[i].ThirdName[0]}." ;
                        worksheet.Cell(i + 4, 2).Value = data[i].HealthGroup;
                        worksheet.Cell(i + 4, 3).Value = data[i].Pegroup;

                    }

                    worksheet.Column(1).Width = 24;
                    worksheet.Column(2).Width = 22;
                    worksheet.Column(3).Width = 22;

                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message);
                alert.ShowDialog();
            }
        }

        private void endDateCb_Checked_1(object sender, RoutedEventArgs e)
        {
            birthDatedp.IsEnabled = true;
            isValid = false;
        }

        private void endDateCb_Unchecked(object sender, RoutedEventArgs e)
        {
            birthDatedp.IsEnabled = false;
            birthDatedp.SelectedDate = null;
            isValid = true;
        }

        private void birthDatedp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (birthDatedp.SelectedDate > DateTime.Now)
            {
                isValid = true;
                birthDatedp.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
            else
            {
                isValid = false;
                birthDatedp.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }
    }
}
