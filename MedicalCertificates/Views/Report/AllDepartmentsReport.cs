using ClosedXML.Excel;
using MedicalCertificates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalCertificates.Views.Report
{
    public static class AllDepartmentsReport
    {
        public static void ExportToExcel(List<TotalReportView> data, string filePath, string title)
        {
            var db = new MedicalCertificatesDbContext();
            var groups = db.GroupsTables.ToList();
            var courses = db.CoursesTables.ToList();
            var departments = db.DepartmentsTables.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Лист1");


                worksheet.Cell(1, 1).Value = title;
                worksheet.Range(1, 1, 1, 4).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetBold()
                    .Font.FontSize = 14;

                int rowIndex = 3;
                var buffer = new List<TotalReportView>();

                foreach (var el in db.PegroupTables)
                {
                    buffer = data.Where(d => d.Pegroup == el.Pegroup).ToList();

                    if (buffer.Count == 0)
                    {
                        continue;
                    }

                    worksheet.Cell(rowIndex, 1).Value = "Медицинская группа: " + el.Pegroup;
                    worksheet.Range(rowIndex, 1, rowIndex, 4).Merge().Style
                    .Font.SetBold().Font.SetItalic();

                    worksheet.Cell(++rowIndex, 1).Value = "ФИО";
                    worksheet.Cell(rowIndex, 2).Value = "Группа";
                    worksheet.Cell(rowIndex, 3).Value = "Курс";
                    worksheet.Cell(rowIndex, 4).Value = "Отделение";

                    worksheet.Range(rowIndex, 1, rowIndex, 4).Style.Font.SetBold();

                    rowIndex++;

                    for (int i = 0; i < buffer.Count; i++, rowIndex++)
                    {
                        worksheet.Cell(rowIndex, 1).Value = $"{buffer[i].SecondName} {buffer[i].FirstName[0]}. {buffer[i].ThirdName[0]}.";
                        worksheet.Cell(rowIndex, 2).Value = buffer[i].GroupName;
                        worksheet.Cell(rowIndex, 3).Value = buffer[i].Course;
                        worksheet.Cell(rowIndex, 4).Value = buffer[i].Department;
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

                workbook.SaveAs(filePath);
            }
        }
    }
}
