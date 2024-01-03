﻿using MedicalCertificates.Models;
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

namespace MedicalCertificates.Views.Delete
{
    /// <summary>
    /// Логика взаимодействия для DeleteCourse.xaml
    /// </summary>
    public partial class DeleteCourse : Window
    {
        MedicalCertificatesDbContext db;
        bool[] isValid;

        public DeleteCourse()
        {
            try
            {
                InitializeComponent();
                db = new MedicalCertificatesDbContext();
                departmentscb.ItemsSource = db.DepartmentsTables.ToList();
                departmentscb.DisplayMemberPath = "Name";

                isValid = new bool[2] { false, false } ;
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

            try
            {
                var alert = new AcceptAlert("Подтверждение", "Вы действительно собираетесь удалить курс?\nВсе группы, а также студенты принадлежащие к данному курсу будут удалены.");
                if (alert.ShowDialog() == true)
                    DeleteDepartmentFromDb();
            }
            catch (Exception ex)
            {
                var alert = new Alert("Добавление", $"Не удалось добавить отделение.\nОшибка: {ex.Message}.\nПовторите попытку.");
                alert.ShowDialog();
            }
        }

        private void DeleteDepartmentFromDb()
        {
            var id = new SqlParameter("@Id", (coursecb.SelectedItem as CoursesTable).CourseId);

            db.Database.ExecuteSqlRaw("SET DATEFORMAT dmy; EXEC DeleteCourse_procedure @Id", id);
            this.DialogResult = true;
            Close();
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
                coursecb.IsEnabled = true;
                coursecb.ItemsSource = db.CoursesTables.Where(c => c.DepartmentId == (departmentscb.SelectedItem as DepartmentsTable).DepartmentId)
                    .OrderBy(c => c.Number)
                    .ToList();
                coursecb.DisplayMemberPath = "Number";
            }
        }

        private void coursecb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (coursecb.SelectedIndex < 0)
            {
                courseBox.BorderBrush = new SolidColorBrush(Colors.Red);
                isValid[1] = false;
            }
            else
            {
                coursecb.BorderBrush = new SolidColorBrush(Colors.Gray);
                isValid[1] = true;
            }
        }
    }
}