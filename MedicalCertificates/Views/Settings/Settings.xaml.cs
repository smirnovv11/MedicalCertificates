using MedicalCertificates.Services;
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
    }
}
