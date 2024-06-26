﻿using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MedicalCertificates.Views.Delete
{
    /// <summary>
    /// Логика взаимодействия для DeleteDepartment.xaml
    /// </summary>
    public partial class DeleteDepartment : Window
    {
        MedCertificatesDbContext db;
        bool isValid;

        public DeleteDepartment()
        {
            try
            {
                InitializeComponent();
                db = new MedCertificatesDbContext();
                departmentscb.ItemsSource = db.DepartmentsTables.ToList();
                departmentscb.DisplayMemberPath = "Name";

                isValid = false;
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
            if (!isValid)
            {
                var alert = new Alert("Ошибка!", "Не все поля заполены или введеные значения неверны.", AlertType.Error);
                alert.ShowDialog();
                return;
            }

            try
            {
                this.PreviewKeyDown -= Window_PreviewKeyDown;
                var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить отделение?\nВсе курсы, группы, а также студенты принадлежащие к данному отделению будут удалены.");
                if (alert.ShowDialog() == true)
                    DeleteDepartmentFromDb();
                else
                    this.PreviewKeyDown += Window_PreviewKeyDown;
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void DeleteDepartmentFromDb()
        {
            if ((departmentscb.SelectedItem as DepartmentsTable).DepartmentId == 1) {
                var alert = new Alert("Ошибка!", "Ошибка: Запрещено удалять данное отделение", AlertType.Error);
                alert.ShowDialog();
                return;
            }

            var dep = new SqlParameter("@Dep", (departmentscb.SelectedItem as DepartmentsTable).DepartmentId);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteDepartment_procedure @Dep", dep);
            this.DialogResult = true;
            Close();
        }

        private void departmentscb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (departmentscb.SelectedIndex < 0)
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid = false;
            }
            else
            {
                departmentBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid = true;
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
