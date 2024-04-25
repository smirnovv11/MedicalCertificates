using ClosedXML.Excel;
using MedicalCertificates.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalCertificates.Views.Report
{
    public static class AllDepartmentsReport
    {
        public static void ExportToExcel(List<TotalReportView> data, string filePath, string title)
        {
            var db = new MedCertificatesDbContext();
            var groups = db.GroupsTables.ToList();
            var courses = db.CoursesTables.ToList();
            var departments = db.DepartmentsTables.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Лист1");


                worksheet.Cell(1, 1).Value = title;
                worksheet.Row(1).Height = 45;
                worksheet.Range(1, 1, 1, 8).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold()
                    .Font.FontSize = 14;

                int rowIndex = 3;
                var buffer = new List<TotalReportView>();

                worksheet.Cell(rowIndex, 1).Value = "Всего выбрано студентов: " + data.Count;
                worksheet.Range(rowIndex, 1, rowIndex, 8).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold()
                    .Font.FontSize = 14;
                rowIndex+=2;

                foreach (var el in db.PegroupTables)
                {
                    buffer = data.Where(d => d.Pegroup == el.Pegroup).ToList();

                    worksheet.Cell(rowIndex, 1).Value = $"Студентов с группой по физкультуре «{el.Pegroup}»: {buffer.Count} ({Math.Round((double)((double)buffer.Count / (double)data.Count * 100.0), 2)}%)";
                    worksheet.Range(rowIndex, 1, rowIndex, 8).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.FontSize = 14;

                    rowIndex++;
                }

                rowIndex++;
                foreach (var el in db.HealthGroupTables)
                {
                    buffer = data.Where(d => d.HealthGroup == el.HealthGroup).ToList();

                    worksheet.Cell(rowIndex, 1).Value = $"Студентов с группой здоровья «{el.HealthGroup}»: {buffer.Count} ({Math.Round((double)((double)buffer.Count / (double)data.Count * 100.0), 2)}%)";
                    worksheet.Range(rowIndex, 1, rowIndex, 8).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.FontSize = 14;

                    rowIndex++;
                }

                rowIndex++;
                foreach (var el in db.PegroupTables)
                {
                    buffer = data.Where(d => d.Pegroup == el.Pegroup).ToList();

                    if (buffer.Count == 0)
                    {
                        continue;
                    }

                    worksheet.Cell(rowIndex, 1).Value = "Медицинская группа: " + el.Pegroup;
                    worksheet.Range(rowIndex, 1, rowIndex, 8).Merge().Style
                    .Font.SetBold().Font.SetItalic().Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    worksheet.Row(rowIndex).Height = 22;

                    worksheet.Cell(++rowIndex, 1).Value = "ФИО Учащегося";
                    worksheet.Cell(rowIndex, 2).Value = "Группа";
                    worksheet.Cell(rowIndex, 3).Value = "Курс";
                    worksheet.Cell(rowIndex, 4).Value = "Отделение";
                    worksheet.Cell(rowIndex, 5).Value = "Группа по физкультуре";
                    worksheet.Cell(rowIndex, 6).Value = "Группа здоровья";
                    worksheet.Cell(rowIndex, 7).Value = "Справка открыта";
                    worksheet.Cell(rowIndex, 8).Value = "Группа годна";

                    worksheet.Range(rowIndex, 1, rowIndex, 8).Style.Font.SetBold();

                    rowIndex++;

                    for (int i = 0; i < buffer.Count; i++, rowIndex++)
                    {
                        worksheet.Cell(rowIndex, 1).Value = $"{buffer[i].SecondName} {buffer[i].FirstName[0]}. {buffer[i].ThirdName[0]}.";
                        worksheet.Cell(rowIndex, 2).Value = buffer[i].GroupName;
                        worksheet.Cell(rowIndex, 3).Value = buffer[i].Course;
                        worksheet.Cell(rowIndex, 4).Value = buffer[i].Department;
                        worksheet.Cell(rowIndex, 5).Value = buffer[i].Pegroup;
                        worksheet.Cell(rowIndex, 6).Value = buffer[i].HealthGroup;
                        worksheet.Cell(rowIndex, 7).Value = buffer[i].IssueDate.ToString("dd.MM.yyyy");
                        worksheet.Cell(rowIndex, 8).Value = buffer[i].ValidDate.ToString("dd.MM.yyyy");
                    }

                    rowIndex++;
                }

                worksheet.Column(1).Width = 25;

                worksheet.Column(2).Width = 20;
                worksheet.Column(2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(3).Width = 16;
                worksheet.Column(3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(4).Width = 24;
                worksheet.Column(4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(5).Width = 24;
                worksheet.Column(5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(6).Width = 20;
                worksheet.Column(6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(7).Width = 19;
                worksheet.Column(7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Column(8).Width = 19;
                worksheet.Column(8).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                workbook.SaveAs(filePath);
            }
        }
    }
}
