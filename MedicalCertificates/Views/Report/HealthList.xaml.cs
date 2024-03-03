using ClosedXML.Excel;
using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
    /// Логика взаимодействия для HealthList.xaml
    /// </summary>
    public partial class HealthList : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public HealthList()
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

            var group = groupcb.SelectedItem as GroupsTable;
            var res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure {(groupcb.SelectedItem as GroupsTable).GroupId}").ToList();

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Сохранить файл Excel";
                if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    string filePath = saveFileDialog.FileName;
                    ExportToExcel(res, filePath, $"Лист здоровья группы {group.Name}");
                }
                else
                    return;

                var sucAlert = new Alert("Успешно", $"Лист здоровья группы {group.Name} был успешно создан.");
                sucAlert.ShowDialog();

                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void ExportToExcel(List<Models.DataGridView> data, string filePath, string title)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");


                    worksheet.Cell(1, 1).Value = title;
                    worksheet.Range(1, 1, 1, 6).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Font.SetBold()
                        .Font.FontSize = 14;

                    worksheet.Cell(3, 1).Value = "Фамилия";
                    worksheet.Cell(3, 2).Value = "Имя";
                    worksheet.Cell(3, 3).Value = "Отчетсво";
                    worksheet.Cell(3, 4).Value = "Группа здоровья";
                    worksheet.Cell(3, 5).Value = "Группа по физкультуре";
                    worksheet.Cell(3, 6).Value = "Справка годна";

                    worksheet.Range(3, 1, 3, 6).Style.Font.SetBold();
                    worksheet.Range(3, 1, 3, 6).Style.Font.SetItalic();

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 4, 1).Value = data[i].SecondName;
                        worksheet.Cell(i + 4, 2).Value = data[i].FirstName;
                        worksheet.Cell(i + 4, 3).Value = data[i].ThirdName;
                        worksheet.Cell(i + 4, 4).Value = data[i].HealthGroup;
                        worksheet.Cell(i + 4, 5).Value = data[i].Pegroup;
                        worksheet.Cell(i + 4, 6).Value = data[i].ValidDate.ToString("dd.MM.yyyy");

                    }

                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 18;
                    worksheet.Column(5).Width = 24;
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
