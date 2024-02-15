using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

namespace MedicalCertificates.Views.Update
{
    /// <summary>
    /// Логика взаимодействия для UpdateCertificate.xaml
    /// </summary>
    public partial class UpdateCertificate : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;
        CertificatesTable item;
        public bool toStudent;

        public UpdateCertificate(int certificateId)
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();

                healthGroupcb.ItemsSource = db.HealthGroupTables.ToList();
                healthGroupcb.DisplayMemberPath = "HealthGroup";

                PEGroupcb.ItemsSource = db.PegroupTables.ToList();
                PEGroupcb.DisplayMemberPath = "Pegroup";

                this.item = db.CertificatesTables.First(c => c.CertificateId == certificateId);

                healthGroupcb.SelectedIndex = healthGroupcb.Items.IndexOf(item.HealthGroup);
                PEGroupcb.SelectedIndex = PEGroupcb.Items.IndexOf(item.Pegroup);

                validDatedp.SelectedDate = item.ValidDate;
                issueDatedp.SelectedDate = item.IssueDate;
                notetb.Text = item.Note;

                isValid = new bool[4] { true, true, true, true };
                toStudent = false;
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

        private void healthGroupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (healthGroupcb.SelectedItem == null || healthGroupcb.SelectedIndex < 0)
            {
                healthGroupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[0] = false;
            }
            else
            {
                healthGroupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[0] = true;
            }
        }

        private void PEGroupcb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PEGroupcb.SelectedItem == null || PEGroupcb.SelectedIndex < 0)
            {
                PEGroupBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
            else
            {
                PEGroupBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[1] = true;
            }
        }

        private void issueDatedp_LostFocus(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate == null || validDatedp.SelectedDate <= issueDatedp.SelectedDate)
            {
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[2] = false;
            }
            else
            {
                issueDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[2] = true;
            }
        }

        private void validDatedp_LostFocus(object sender, RoutedEventArgs e)
        {
            if (validDatedp.SelectedDate == null || validDatedp.SelectedDate <= issueDatedp.SelectedDate)
            {
                validDatedp.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[3] = false;
            }
            else
            {
                validDatedp.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[3] = true;
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var el in isValid)
                if (!el)
                {
                    var alert = new Alert("Ошибка!", "Не все поля заполены или введеные значения неверны.", AlertType.Error);
                    alert.ShowDialog();
                    return;
                }

            try
            {
                AddStudentToDb();

                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Изменение", $"Не удалось обновить справку.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void AddStudentToDb()
        {
            var healthId = new SqlParameter("@Health", (healthGroupcb.SelectedItem as HealthGroupTable).HealthGroupId);
            var peId = new SqlParameter("@PE", (PEGroupcb.SelectedItem as PegroupTable).PegroupId);
            var issueDate = new SqlParameter("@Issue", issueDatedp.SelectedDate);
            var validDate = new SqlParameter("@Valid", validDatedp.SelectedDate);
            var note = new SqlParameter("@Note", notetb.Text);
            var id = new SqlParameter("@Id", item.CertificateId);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC UpdateCertificate_procedure @Id, @Health, @PE, @Issue, @Valid, @Note", id, healthId, peId, issueDate, validDate, note);
        }

        private void ToStudentButton_Click(object sender, RoutedEventArgs e)
        {
            toStudent = true;
            DialogResult = true;
            this.Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                YesButton.Focus();
                this.YesButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void clearValidDateButton_Click(object sender, RoutedEventArgs e)
        {
            validDatedp.SelectedDate = null;
            isValid[3] = false;
        }

        private void addYear_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(12);
            }
            isValid[3] = true;
        }

        private void add6Month_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(6);
            }
            isValid[3] = true;
        }

        private void add3Month_Click(object sender, RoutedEventArgs e)
        {
            if (issueDatedp.SelectedDate != null)
            {
                validDatedp.SelectedDate = issueDatedp.SelectedDate.Value.AddMonths(3);
            }
            isValid[3] = true;
        }
    }
}
