using MedicalCertificates.Services;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MedicalCertificates.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            dbnameTb.Text = JsonServices.ReadByProperty("dbname");
            
            var period = Int32.Parse(JsonServices.ReadByProperty("warningPeriod"));
            switch (period)
            {
                case 1:
                    warningPeriodCb.SelectedIndex = 0;
                    break;
                case 3:
                    warningPeriodCb.SelectedIndex = 1;
                    break;
                case 4:
                    warningPeriodCb.SelectedIndex = 2;
                    break;
                case 6:
                    warningPeriodCb.SelectedIndex = 3;
                    break;
                default:
                    warningPeriodCb.SelectedIndex = 1;
                    break;
            }

            autoCoursesCb.IsChecked = Boolean.Parse(JsonServices.ReadByProperty("autoCourses"));
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
                this.saveBtn.Focus();
                this.saveBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dbnameTb.Text.Trim() != JsonServices.ReadByProperty("dbname"))
            {
                JsonServices.Write("dbname", dbnameTb.Text.Trim());
            }

            if (warningPeriodCb.Text.Substring(0, 1) != JsonServices.ReadByProperty("warningPeriod"))
            {
                JsonServices.Write("warningPeriod", warningPeriodCb.Text.Substring(0, 1));
            }

            JsonServices.Write("autoCourses", autoCoursesCb.IsChecked.ToString());

            DialogResult = true;
            Close();
        }

        private async void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPath = ShowFolderBrowserDialog("Укажите папку создания резервной копии");

                if (string.IsNullOrEmpty(folderPath))
                {
                    this.Focus();
                    return;
                }

                this.Focus();

                progressBar.Visibility = Visibility.Visible;
                progressBar.Value = 70;

                var conn = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=MedicalCertificatesDb;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";
                await Task.Run(() => BackupDatabaseAndLog(conn, "MedicalCertificatesDb", folderPath));

                progressBar.Visibility = Visibility.Hidden;

                var succAlert = new Alert("Успешно", "Резервная копия создана успешно.", AlertType.Info);
                succAlert.ShowDialog();
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Hidden;
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private static string ShowFolderBrowserDialog(string title)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = title;
            Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult result = dialog.ShowDialog();
            if (result == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            return null;
        }

        private static void BackupDatabaseAndLog(string connectionString, string databaseName, string folderPath)
        {
            var backupDatabaseCommand = "BACKUP DATABASE @databaseName TO DISK = @backupDatabasePath WITH INIT";
            var backupLogCommand = "BACKUP LOG @databaseName TO DISK = @backupLogPath WITH INIT";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(backupDatabaseCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@databaseName", databaseName);
                    cmd.Parameters.AddWithValue("@backupDatabasePath", System.IO.Path.Combine(folderPath, $"{databaseName}_db.bak"));
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(backupLogCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@databaseName", databaseName);
                    cmd.Parameters.AddWithValue("@backupLogPath", System.IO.Path.Combine(folderPath, $"{databaseName}_log.bak"));
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private async void ImportLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var askAlert = new AcceptAlert("Внимание", "Восстановление базы данных удалит все текущие данные. Вы желаете продолжить?", AlertType.Warning);
                if (askAlert.ShowDialog() == true)
                {
                    var folderPath = ShowFolderBrowserDialog("Укажите папку загрузки резервной копии");

                    if (string.IsNullOrEmpty(folderPath))
                    {
                        this.Focus();
                        return;
                    }

                    this.Focus();

                    var conn = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=master;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";

                    progressBar.Visibility = Visibility.Visible;

                    var progress = new Progress<int>(value => progressBar.Value = value);
                    await Task.Run(() => RestoreDatabaseAndLog(conn, "MedicalCertificatesDb", folderPath, progress));

                    progressBar.Visibility = Visibility.Hidden;

                    var succAlert = new Alert("Успешно", "Резервная копия успешно импортированно. База данных восстановленна.", AlertType.Info);
                    succAlert.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Hidden;
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }


        private static void RestoreDatabaseAndLog(string connectionString, string databaseName, string folderPath, IProgress<int> progress)
        {
            var createDatabaseCommand = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}') CREATE DATABASE {databaseName}";
            var setSingleUserModeCommand = $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            var restoreDatabaseCommand = "RESTORE DATABASE @databaseName FROM DISK = @backupDatabasePath WITH REPLACE, NORECOVERY";
            var restoreLogCommand = "RESTORE LOG @databaseName FROM DISK = @backupLogPath WITH NORECOVERY";
            var recoverDatabaseCommand = $"RESTORE DATABASE {databaseName} WITH RECOVERY";
            var setMultiUserModeCommand = $"ALTER DATABASE {databaseName} SET MULTI_USER";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(createDatabaseCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                progress.Report(20);
                using (var cmd = new SqlCommand(setSingleUserModeCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(restoreDatabaseCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@databaseName", databaseName);
                    cmd.Parameters.AddWithValue("@backupDatabasePath", System.IO.Path.Combine(folderPath, $"{databaseName}_db.bak"));
                    cmd.ExecuteNonQuery();
                }
                progress.Report(45);
                using (var cmd = new SqlCommand(restoreLogCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@databaseName", databaseName);
                    cmd.Parameters.AddWithValue("@backupLogPath", System.IO.Path.Combine(folderPath, $"{databaseName}_log.bak"));
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(recoverDatabaseCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                progress.Report(88);
                using (var cmd = new SqlCommand(setMultiUserModeCommand, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                progress.Report(100);
            }
        }
    }
}
