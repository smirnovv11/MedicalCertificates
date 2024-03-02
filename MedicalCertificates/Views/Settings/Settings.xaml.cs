using MedicalCertificates.Services;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
                saveBtn.Focus();
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

            DialogResult = true;
            Close();
        }

        private void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ShowSaveFileDialog();
                var conn = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=MedicalCertificatesDb;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";
                BackupDatabase(conn, "MedicalCertificatesDb", path);
                BackupLog(conn, "MedicalCertificatesDb", path);
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private static string ShowSaveFileDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Backup Files (*.bak)|*.bak";
            saveFileDialog.DefaultExt = "bak";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        private static void BackupDatabase(string connectionString, string databaseName, string backupFilePath)
        {
            var backupCommand = "BACKUP DATABASE @databaseName TO DISK = @backupFilePath";
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(backupCommand, conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@databaseName", databaseName);
                cmd.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BackupLog(string connectionString, string databaseName, string backupFilePath)
        {
            var backupCommand = "BACKUP LOG @databaseName TO DISK = @backupFilePath";
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(backupCommand, conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@databaseName", databaseName);
                cmd.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                cmd.ExecuteNonQuery();
            }
        }

        private void ImportLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ShowOpenFileDialog();
                var conn = $"Data Source={JsonServices.ReadByProperty("dbname")};Initial Catalog=MedicalCertificatesDb;Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=true;";

                ImportLogToDatabase(conn, path);
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message, AlertType.Error);
                alert.ShowDialog();
            }
        }

        private static string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Log Files (*.log)|*.log";
            openFileDialog.DefaultExt = "log";
            bool? dialogOK = openFileDialog.ShowDialog();
            if (dialogOK == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        private static void ImportLogToDatabase(string connectionString, string logFilePath)
        {
            string[] logLines = File.ReadAllLines(logFilePath);
            //using (var conn = new SqlConnection(connectionString))
            //{
            //    conn.Open();
            //    foreach (string logLine in logLines)
            //    {
            //        string[] logParts = logLine.Split(',');
            //        int logId = int.Parse(logParts[0]);
            //        DateTime logDate = DateTime.Parse(logParts[1]);
            //        string logMessage = logParts[2];

            //        string insertCommand = "INSERT INTO " + tableName + " (LogId, LogDate, LogMessage) VALUES (@LogId, @LogDate, @LogMessage)";
            //        using (var cmd = new SqlCommand(insertCommand, conn))
            //        {
            //            cmd.Parameters.AddWithValue("@LogId", logId);
            //            cmd.Parameters.AddWithValue("@LogDate", logDate);
            //            cmd.Parameters.AddWithValue("@LogMessage", logMessage);
            //            cmd.ExecuteNonQuery();
            //        }
            //    }
            //}
        }
    }
}
