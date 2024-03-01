using ClosedXML.Excel;
using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        MedicalCertificatesDbContext db;
        ReportType type;

        public ReportSettings(ReportType type)
        {
            InitializeComponent();

            this.type = type;
            switch (type)
            {
                case ReportType.Group:
                    break;
                case ReportType.Course:
                    groupPanel.Visibility = Visibility.Collapsed;
                    break;
                case ReportType.Department:
                    groupPanel.Visibility = Visibility.Collapsed;
                    coursePanel.Visibility = Visibility.Collapsed;
                    break;
                case ReportType.TotalReport:
                    groupPanel.Visibility = Visibility.Collapsed;
                    coursePanel.Visibility = Visibility.Collapsed;
                    departmentPanel.Visibility = Visibility.Collapsed;
                    break;
            }

            db = new MedicalCertificatesDbContext();
            if (type != ReportType.TotalReport)
            {
                departmentcb.ItemsSource = db.DepartmentsTables.ToList();
                departmentcb.DisplayMemberPath = "Name";
            }
            isValid = true;
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

                courseCb.SelectedItem = null;
                courseCb.IsEnabled = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);

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
                courseCb.BorderBrush = new SolidColorBrush(Colors.Red);

                groupcb.SelectedItem = null;
                groupcb.IsEnabled = false;
            }
            else
            {
                courseCb.BorderBrush = new SolidColorBrush(Colors.Gray);

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
                groupBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                groupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
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
                    title = $"Отчет по отделению ({(departmentcb.SelectedItem as DepartmentsTable).Name})";
                    break;
                case ReportType.Course:
                    title = $"Отчет по курсу ({(courseCb.SelectedItem as CoursesTable).Number}), отделение {(departmentcb.SelectedItem as DepartmentsTable).Name}";
                    res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsCourse_procedure {(courseCb.SelectedItem as CoursesTable).CourseId}").ToList();
                    break;
                case ReportType.Group:
                    title = $"Отчет по группе ({(groupcb.SelectedItem as GroupsTable).Name})";
                    res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure {(groupcb.SelectedItem as GroupsTable).GroupId}").ToList();
                    break;
            }

            // TODO: Проверки на дату и типы справок

            if (cb1.IsChecked == true && cb3.IsChecked == false)
            {
                res.Where(d => DateTime.Compare(d.ValidDate, DateTime.Now) == 1);
            } 
            else if (cb1.IsChecked == false && cb3.IsChecked == true)
            {
                res.Where(d => d.ValidDate <= DateTime.Now);
            }

            if (birthDatedp.SelectedDate != null)
            {
                res.Where(d => d.ValidDate <= birthDatedp.SelectedDate.Value).ToList();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Сохранить файл Excel";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                ExportToExcel(res, filePath, title);
            }

            Close();
        }

        private void ExportToExcel(List<DataGridView> data, string filePath, string title)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");


                worksheet.Cell(1, 1).Value = title;
                worksheet.Range(1, 1, 1, 6).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetBold();

                worksheet.Cell(2, 1).Value = "Фамилия";
                worksheet.Cell(2, 2).Value = "Имя";
                worksheet.Cell(2, 3).Value = "Отчетсво";
                worksheet.Cell(2, 4).Value = "Группа здоровья";
                worksheet.Cell(2, 5).Value = "Группа по физкультуре";
                worksheet.Cell(2, 6).Value = "Справка годна";

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(i + 3, 1).Value = data[i].SecondName;
                    worksheet.Cell(i + 3, 2).Value = data[i].FirstName;
                    worksheet.Cell(i + 3, 3).Value = data[i].ThirdName;
                    worksheet.Cell(i + 3, 4).Value = data[i].HealthGroup;
                    worksheet.Cell(i + 3, 5).Value = data[i].Pegroup;
                    worksheet.Cell(i + 3, 6).Value = data[i].ValidDate;

                }

                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 18;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 16;

                workbook.SaveAs(filePath);
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
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                isValid = false;
                birthDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
