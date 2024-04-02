using ClosedXML.Excel;
using MedicalCertificates.Models;
using MedicalCertificates.Services.Alert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalCertificates.Views.Report
{
    public static class ChangeHealthReport
    {
        public static void ExportToExcel(List<HealthGroupChangesView> data, string filePath, string title)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Лист1");


                    worksheet.Cell(1, 1).Value = title;
                    worksheet.Range(1, 1, 1, 8).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Font.SetBold()
                        .Font.FontSize = 14;

                    worksheet.Cell(3, 1).Value = "Фамилия";
                    worksheet.Cell(3, 2).Value = "Имя";
                    worksheet.Cell(3, 3).Value = "Отчетсво";
                    worksheet.Cell(3, 4).Value = "Текущая группа здоровья";
                    worksheet.Cell(3, 5).Value = "Текущая группа по физкультуре";
                    worksheet.Cell(3, 6).Value = "Предыдущая группа здоровья";
                    worksheet.Cell(3, 7).Value = "Предыдущая группа по физкультуре";
                    worksheet.Cell(3, 8).Value = "Дата изменения";

                    worksheet.Range(3, 1, 3, 8).Style.Font.SetBold();
                    worksheet.Range(3, 1, 3, 8).Style.Font.SetItalic();

                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 4, 1).Value = data[i].SecondName;
                        worksheet.Cell(i + 4, 2).Value = data[i].FirstName;
                        worksheet.Cell(i + 4, 3).Value = data[i].ThirdName;
                        worksheet.Cell(i + 4, 4).Value = data[i].CurrHealth;
                        worksheet.Cell(i + 4, 5).Value = data[i].CurrPe;
                        worksheet.Cell(i + 4, 6).Value = data[i].PrevHealth;
                        worksheet.Cell(i + 4, 7).Value = data[i].PrevPe;
                        worksheet.Cell(i + 4, 8).Value = data[i].UpdateDate.ToString("dd.MM.yyyy");

                    }

                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 26;
                    worksheet.Column(5).Width = 33;
                    worksheet.Column(6).Width = 32;
                    worksheet.Column(7).Width = 38;
                    worksheet.Column(8).Width = 17;

                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                var alert = new Alert("Ошибка!", "Ошибка: " + ex.Message);
                alert.ShowDialog();
            }
        }
    }
}
