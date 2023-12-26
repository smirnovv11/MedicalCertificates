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

namespace MedicalCertificates.Services.Alert
{
    /// <summary>
    /// Логика взаимодействия для Alert.xaml
    /// </summary>
    public partial class Alert : Window
    {
        public Alert(string label, string text)
        {
            InitializeComponent();
            Title.Content = label;
            Message.Text = text;
        }
        public Alert(string label, string text, AlertType type) : this(label, text)
        {
            switch (type)
            {
                case AlertType.Info:
                    break;
                case AlertType.Warning:
                    Back.Background = (Brush)new BrushConverter().ConvertFrom("#ebb434");
                    Title.Foreground = (Brush)new BrushConverter().ConvertFrom("#1f1300");
                    ExitButton.Foreground = Title.Foreground;
                    break;
                case AlertType.Error:
                    Back.Background = (Brush)new BrushConverter().ConvertFrom("#a10000");
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

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
