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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ClosedXML.Excel;
using System.Data;
using System.IO;
using MedicalCertificates.Views.Settings;
using MedicalCertificates.Views.Report;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace MedicalCertificates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MedCertificatesDbContext db;
        int currGroupId;
        int lastGroupId;
        int currStudentId;
        DbSet<GroupsTable> groups;
        DbSet<StudentsTable> students;

        public MainWindow()
        {
            try
            {
                // Путь к файлу настроек
                var jsonPath = "settings.json";
                // Если файл настроек не существует, создаем его с начальными значениями
                if (!File.Exists(jsonPath))
                {
                    JsonServices.Write("dbname", "(localdb)\\MSSQLLocalDB");
                    JsonServices.Write("warningPeriod", "3");
                    JsonServices.Write("autoCourses", "true");
                    JsonServices.Write("lastOpenTableId", "1");

                    var settWind = new Settings(true);
                    settWind.ShowDialog();
                }

                EnsureCreateDb.EnsureAndCreate();

                // Инициализация контекста базы данных
                db = new MedCertificatesDbContext();
                InitializeComponent();

                // Установка культуры и формата даты для текущего потока
                CultureInfo cultureInfo = new CultureInfo("ru-RU");
                DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
                dateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                cultureInfo.DateTimeFormat = dateTimeFormat;
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                // Получение доступа к таблицам базы данных
                groups = db.GroupsTables;
                students = db.StudentsTables;

                currStudentId = -1;
                currGroupId = 1;
                lastGroupId = 1;

                if (!string.IsNullOrEmpty(JsonServices.ReadByProperty("lastOpenTableId"))
                    && int.TryParse(JsonServices.ReadByProperty("lastOpenTableId"), out currGroupId)
                    && currGroupId > 0
                    && db.GroupsTables.FirstOrDefault(g => g.GroupId == currGroupId) != null)
                    currGroupId = int.Parse(JsonServices.ReadByProperty("lastOpenTableId"));
                else
                {
                    JsonServices.Write("lastOpenTableId", "1");
                    currGroupId = 1;
                }

                TableLabel.Text = $"Листок здоровья группы {db.GroupsTables.First(g => g.GroupId == currGroupId).Name} ({db.CoursesTables.First(c => c.CourseId == db.GroupsTables.First(g => g.GroupId == currGroupId).CourseId).Number} курс)";

                // Обновление данных
                UpdateAllDbData();


            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке, если не удалось подключиться к базе данных
                var alert = new Alert("Ошибка!", "Ошибка: Не удалось подключится к базе данных либо база данных повреждена.\nПодробности: " + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Установка размеров и положения окна при загрузке
            double taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight - taskbarHeight;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Перемещение окна при нажатии левой кнопкой мыши
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            // Сворачивание окна при нажатии на кнопку "Свернуть"
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрытие приложения при нажатии на кнопку "Закрыть"
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (lastGroupId > 0)
                JsonServices.Write("lastOpenTableId", lastGroupId.ToString());
            else
                JsonServices.Write("lastOpenTableId", "1");
            Application.Current.Shutdown();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == "Поиск")
            {
                searchBox.Text = "";
                searchBox.Foreground = new SolidColorBrush(Colors.Black);

                searchBox.Focus();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "Поиск";
                searchBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void ReportMenu_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender).ContextMenu;
            contextMenu.PlacementTarget = sender as UIElement;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private void UpdateAllDbData(bool foreceUpdate = false)
        {
            try
            {
                UpdateTreeData(true);
                UpdateGridData();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Не удалось обновить данные базы данных. Повторите попытку или восстановите базу данных." + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void UpdateTreeData(bool forceUpdate = false)
        {
            if (forceUpdate)
            {
                db = new MedCertificatesDbContext();
                TreeMenu.ItemsSource = db.DepartmentsTables
                              .Include(d => d.CoursesTables.OrderBy(c => c.Number))
                              .ThenInclude(c => c.GroupsTables)
                              .ThenInclude(g => g.StudentsTables.OrderBy(s => s.SecondName).ThenBy(s => s.FirstName))
                              .ToList();
            }
            else if (currGroupId != null && currGroupId >= 0)
            {
                db = new MedCertificatesDbContext();
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
                db = new MedCertificatesDbContext();

                res = db.DataGridViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsGroup_procedure {currGroupId}").ToList();
            }
            else if (currStudentId != null && currStudentId >= 0)
            {
                db = new MedCertificatesDbContext();

                res = db.DataGridViews.Where(s => s.StudentId == currStudentId).ToList();
            }
            var unSortGroup = db.GroupsTables.First(g => g.CourseId
                                == db.CoursesTables.First(c => c.DepartmentId
                                == db.DepartmentsTables.First(d => d.DepartmentId == 1)
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
            try
            {
                var text = (dataGrid.SelectedItem as DataGridView).Note;

                if (text == null || text.Length <= 0)
                    text = "Примечание отсутствует.";

                var alert = new Alert("Примечание", text);
                alert.ShowDialog();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка отоброжения подробностей. Повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void GroupTree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var group = ((sender as Button).DataContext as GroupsTable);
                currGroupId = group.GroupId;
                lastGroupId = currGroupId;
                currStudentId = -1;

                UpdateGridData();

                TableLabel.Text = $"Листок здоровья группы {group.Name} ({group.Course.Number} курс)";
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка отображения группы. Повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void StudentTree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var student = ((sender as Button).DataContext as StudentsTable);
                currStudentId = student.StudentId;
                lastGroupId = currGroupId;
                currGroupId = -1;

                UpdateGridData();

                TableLabel.Text = $"Листок здоровья учащегося ({student.SecondName + " " + student.FirstName + " " + student.ThirdName})";
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка отображения истории студента. Повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        #region Addition

        private void StudentAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddStudent();
            if (wind.ShowDialog() == true)
                UpdateAllDbData(true);
        }
        private void DepartmentAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddDepartment();
            if (wind.ShowDialog() == true)
                UpdateTreeData(true);
        }
        private void CourseAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddCourse();
            if (wind.ShowDialog() == true)
                UpdateTreeData(true);
        }
        private void GroupAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddGroup();
            if (wind.ShowDialog() == true)
                UpdateTreeData(true);
        }
        private void CertificateAddition_Click(object sender, RoutedEventArgs e)
        {
            var wind = new AddCertificate();
            if (currStudentId >= 0)
            {
                wind = new AddCertificate(db.StudentsTables.First(s => s.StudentId == currStudentId));
            }
            
            if (currGroupId >= 0)
            {
                wind = new AddCertificate(db.GroupsTables.First(s => s.GroupId == currGroupId));
            }

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
                UpdateAllDbData(true);
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteCourse();
            if (wind.ShowDialog() == true)
                UpdateAllDbData(true);
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            var wind = new DeleteGroup();
            if (wind.ShowDialog() == true)
                UpdateAllDbData(true);
        }

        #endregion

        #region Updating

        private void UpdateDepartment_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateDepartment();
            if (wind.ShowDialog() == true)
                UpdateTreeData(true);
        }

        private void UpdateCourse_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateCourse();
            if (wind.ShowDialog() == true)
                UpdateTreeData(true);
        }

        private void UpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            var wind = new UpdateGroup();
            if (wind.ShowDialog() == true)
                UpdateAllDbData(true);
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
                lastGroupId = currGroupId;
                currGroupId = -1;

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
                        var group = db.GroupsTables.First(g => g.CourseId
                                == db.CoursesTables.First(c => c.DepartmentId
                                == db.DepartmentsTables.First(d => d.DepartmentId == 1)
                                .DepartmentId)
                                .CourseId)
                                .GroupId;

                        for (int row = 2; row <= rowCount; ++row)
                        {
                            var groupId = new SqlParameter("@GroupId", group);
                            var firstName = new SqlParameter("@FirstName", worksheet.Cell(row, 2).GetValue<String>());
                            var secondName = new SqlParameter("@SecondName", worksheet.Cell(row, 1).GetValue<String>());
                            var thirdName = new SqlParameter("@ThirdName", worksheet.Cell(row, 3).GetValue<String>());
                            var birthDate = new SqlParameter("@BirthDate", DateTime.Parse(worksheet.Cell(row, 4).GetValue<String>()));

                            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateStudent_procedure @GroupId, @FirstName, @SecondName, @ThirdName, @BirthDate", groupId, firstName, secondName, thirdName, birthDate);
                            var studentId = db.StudentsTables.OrderByDescending(s => s.StudentId).First().StudentId;

                            var studentParam = new SqlParameter("@StudentId", studentId);
                            var healthId = new SqlParameter("@Health", 1);
                            var peId = new SqlParameter("@PE", 6);

                            var issueDate = new SqlParameter("@Issue", DateTime.Now);
                            if (!string.IsNullOrEmpty(worksheet.Cell(row, 5).GetValue<String>()))
                            {
                                issueDate = new SqlParameter("@Issue", DateTime.Parse(worksheet.Cell(row, 5).GetValue<String>()));
                            }
                            
                            var validDate = new SqlParameter("@Valid", DateTime.Now.AddYears(1));
                            if (!string.IsNullOrEmpty(worksheet.Cell(row, 6).GetValue<String>()))
                            {
                                validDate = new SqlParameter("@Valid", DateTime.Parse(worksheet.Cell(row, 6).GetValue<String>()));
                            }

                            var note = new SqlParameter("@Note", "");

                            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC CreateCertificate_procedure @StudentId, @Health, @PE, @Issue, @Valid, @Note", studentParam, healthId, peId, issueDate, validDate, note);
                        }

                        currGroupId = group;
                        lastGroupId = currGroupId;
                        currStudentId = -1;

                        UpdateGridData();

                        TableLabel.Text = $"Листок здоровья неопределенных по группам учащихся";
                    }
                }
            }
            catch (Exception ex) 
            {
                var alert = new Alert("Ошибка!", "Не удалось импортировать данные из файла. Возможно данные повреждены или файл используется другой программой.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchForTbData();
            }
        }

        private void SearchForTbData()
        {
            try
            {
                if (searchBox.Text.Length >= 2 && searchBox.Text != "Поиск")
                {
                    ContextMenu contextMenu = searchBox.ContextMenu;
                    contextMenu.PlacementTarget = searchBox as UIElement;
                    contextMenu.Placement = PlacementMode.Bottom;
                    contextMenu.Items.Clear();

                    var res = students.Select(g => g.SecondName + " " + g.FirstName.Substring(0, 1) + ". " + g.ThirdName.Substring(0, 1) + ".").Where(s => s.ToLower().Contains(searchBox.Text.ToLower())).ToList()
                        .Concat(groups.Select(g => g.Name).Where(g => g.ToLower().Contains(searchBox.Text.ToLower())).ToList())
                        .Concat(db.PegroupTables.Select(p => p.Pegroup).Where(p => p.ToLower().Contains(searchBox.Text.ToLower())).ToList())
                        .ToList();

                    foreach (string el in res)
                    {
                        MenuItem item = new MenuItem();
                        item.Header = el;
                        item.StaysOpenOnClick = true;
                        item.Style = App.Current.TryFindResource("PrimaryMenuItem") as Style;
                        contextMenu.Items.Add(item);

                        item.Click += OpenSearchResult;
                    }

                    contextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка поиска. Повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void OpenSearchResult(object sender, RoutedEventArgs e)
        {
            try
            {
                var header = (sender as MenuItem).Header.ToString();
                string[] arr = header.Split(' ');
                if (arr.Length == 3)
                {
                    var student = db.StudentsTables.FirstOrDefault(s => s.SecondName == arr[0]
                    && s.FirstName.StartsWith(arr[1].Substring(0, 1)) && s.ThirdName.StartsWith(arr[2].Substring(0, 1)));
                    currStudentId = student.StudentId;
                    lastGroupId = currGroupId;
                    currGroupId = -1;

                    UpdateGridData();

                    TableLabel.Text = $"Листок здоровья учащегося ({student.SecondName + " " + student.FirstName + " " + student.ThirdName})";
                }
                else 
                {
                    if (db.PegroupTables.FirstOrDefault(p => p.Pegroup == header) != null)
                    {
                        var PEgroup = db.PegroupTables.FirstOrDefault(p => p.Pegroup == header);

                        db = new MedCertificatesDbContext();
                        var res = db.DataGridViews.Where(d => d.Pegroup == PEgroup.Pegroup).ToList();
                        dataGrid.ItemsSource = res;

                        TableLabel.Text = $"Результаты поиска по группе '{header}'";
                    }
                    else
                    {
                        var group = db.GroupsTables.FirstOrDefault(g => g.Name == arr[0]);
                        currGroupId = group.GroupId;
                        lastGroupId = currGroupId;
                        currStudentId = -1;

                        UpdateGridData();

                        TableLabel.Text = $"Листок здоровья группы {group.Name} ({db.CoursesTables.FirstOrDefault(c => c.CourseId == group.CourseId).Number} курс)";
                    }
                }
                searchBox.Clear();
                dataGrid.Focus();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Не удалось отобразить результаты поиска. Повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchForTbData();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var wind = new Settings();
            if (wind.ShowDialog() == true)
            {
                UpdateAllDbData();
            }
        }

        private void GroupReport_Click(object sender, RoutedEventArgs e)
        {
            OpenReportSettings(ReportType.Group);
        }
        private void CourseReport_Click(object sender, RoutedEventArgs e)
        {
            OpenReportSettings(ReportType.Course);
        }
        private void DepartmentReport_Click(object sender, RoutedEventArgs e)
        {
            OpenReportSettings(ReportType.Department);
        }

        private void AllReport_Click(object sender, RoutedEventArgs e)
        {
            OpenReportSettings(ReportType.TotalReport);
        }

        private void ChangedHealthReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Сохранить файл Excel";

                string filePath = "";
                var res = db.HealthGroupChangesViews.OrderByDescending(s => s.UpdateDate).ToList();
                if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    filePath = saveFileDialog.FileName;
                    ChangeHealthReport.ExportToExcel(res, filePath, "Отчет по изменившимся группам здоровья");
                }
                else
                    return;

                var alert = new Alert("Успешно", "Отчет по изменившимся группам здоровья был успешно создан.");
                alert.ShowDialog();

                //Открываем файл в Excel
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Не удалось посторить отчет. Повторите попытку.");
                alert.ShowDialog();
            }
        }

        private void OpenReportSettings(ReportType type)
        {
            var wind = new ReportSettings(type);
            wind.ShowDialog();
        }

        private void AllDepReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Сохранить файл Excel";

                string filePath = "";
                var res = db.TotalReportViews.FromSqlRaw($"SET DATEFORMAT dmy; EXEC ReceiveStudentsForReport_procedure").ToList();
                if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    filePath = saveFileDialog.FileName;
                    AllDepartmentsReport.ExportToExcel(res, filePath, "Отчет по группам здоровья для отделений");
                }
                else
                    return;

                var alert = new Alert("Успешно", "Отчет по группам здоровья для отделений был успешно создан.");
                alert.ShowDialog();

                //Открываем файл в Excel
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка!\", \"Не удалось посторить отчет. Повторите попытку.");
                alert.ShowDialog();
            }
        }

        private void HealthList_Click(object sender, RoutedEventArgs e)
        {
            OpenReportSettings(ReportType.ShortGroupPE);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists("Help.chm"))
                {
                    var alert = new Alert("Ошибка!", "Ошибка: Файл справки отсутствует, восстановите программу через приложение-установщик");
                    alert.ShowDialog();
                }

                System.Windows.Forms.Help.ShowHelp(null, "Help.chm");
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Файл справки отсутствует или поврежден. Восстановите данные программы и повторите попытку.", AlertType.Error);
                alert.ShowDialog();
            }
        }
        
    }
}
