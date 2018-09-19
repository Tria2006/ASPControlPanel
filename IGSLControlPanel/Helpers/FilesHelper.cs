using ClosedXML.Excel;
using DBModels.Models;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class FilesHelper
    {
        private const string PathToExcelFiles = "ExcelFiles";

        public async Task UploadFile(IFormFile file, IGSLContext context)
        {
            if (file == null || file.Length == 0)
                return;

            var workbook = new XLWorkbook(file.OpenReadStream());
            foreach (var worksheet in workbook.Worksheets)
            {
                await ReadWorksheetData(worksheet, context);
            }
        }

        public string CreateExcel(Tariff tariff, IGSLContext context)
        {
            const string fileName = "tempFile.xlsx";
            var path = Path.Combine(
                Directory.GetCurrentDirectory(), PathToExcelFiles,
                fileName);
            using (var workBook = new XLWorkbook())
            {
                foreach (var link in tariff.RiskFactorsTariffLinks)
                {
                    var factorValues = context.FactorValues.Include(x => x.Values).Where(x =>
                        x.RiskFactorId == link.RiskFactorId && x.TariffId == tariff.Id && !x.IsDeleted);
                    link.RiskFactor.FactorValues = factorValues.ToList();
                    AddWorkSheet(tariff, link.RiskFactor, workBook, context);
                }
                workBook.SaveAs(path);
            }
            return path;
        }

        private void AddWorkSheet(Tariff tariff, RiskFactor factor, IXLWorkbook workbook, IGSLContext context)
        {
            var worksheet = workbook.Worksheets.Add(factor.Name);
            // проставляем id фактора риска
            worksheet.Cell(1, 1).Value = factor.Id;

            // собираем список рисков
            var risks = new List<Risk>();
            foreach (var ruleLink in tariff.InsRuleTariffLink)
            {
                foreach (var riskLink in ruleLink.InsRule.LinksToRisks)
                {
                    if (risks.TrueForAll(x => x.Id != riskLink.Risk.Id))
                        risks.Add(riskLink.Risk);
                }
            }

            // стартовая строка для рисков = 3, т.к. по 1-й идут id фактора риска и id значений факторов риска
            // на 2-й идут названия значений факторов риска
            var currentRow = 3;

            // заполняем строки (id | Name)
            foreach (var risk in risks)
            {
                worksheet.Cell(currentRow, 1).Value = risk.Id;
                worksheet.Cell(currentRow, 2).Value = risk.Name;
                currentRow++;
            }

            // стартовая строка для значений фактора риска = 3, т.к. по 1-й идут id риска, а на 2-й идут названия значений факторов риска
            var currentColumn = 3;
            // заполняем столбцы (id | Name)
            foreach (var factorValue in factor.FactorValues)
            {
                worksheet.Cell(1, currentColumn).Value = factorValue.Id;
                worksheet.Cell(2, currentColumn).Value = factorValue.Name;
                // заполняем значения
                for (var i = 3; i < worksheet.RowCount(); i++)
                {
                    // получаем id риска из первой ячейки
                    var rowid = worksheet.Cell(i, 1).Value.ToString();
                    Guid.TryParse(rowid, out var riskId);
                    if (riskId == Guid.Empty) break;
                    var coef = factorValue.Values.SingleOrDefault(x =>
                        x.FactorValueId == factorValue.Id && x.RiskId == riskId);
                    if (coef != null)
                    {
                        // проставляем значение в ячейку
                        worksheet.Cell(i, currentColumn).Value = coef.Value;
                    }
                }
                currentColumn++;
            }

            // скрываем стрщки с id
            worksheet.Row(1).Collapse();
            worksheet.Column(1).Collapse();
        }

        private async Task ReadWorksheetData(IXLWorksheet worksheet, IGSLContext context)
        {
            for (int i = 3; i < worksheet.RowCount(); i++)
            {
                var row = worksheet.Row(i);
                // пытаемся получить id риска в первой ячейке строки
                Guid.TryParse(row.Cell(1).Value.ToString(), out var riskId);

                // если id получен, то это значит, что риск был добавлен из БД,
                // если нет, то значит юзер добавил строку сам и надо создавать риск
                if (riskId != Guid.Empty)
                {
                    //var risk = await context.Risks.FindAsync(riskId);




                    // TODO: если есть id, но не найден риск




                    for (int j = 3; j < worksheet.ColumnCount(); j++)
                    {
                        // пытаемся получить id риска в первой ячейке столбца
                        Guid.TryParse(worksheet.Cell(1, j).Value.ToString(), out var factorValueId);

                        // если id получен, то это значит, что значение фактора риска было добавлено из БД,
                        // если нет, то значит юзер добавил строку сам и надо создавать значение фактора риска
                        if (factorValueId != Guid.Empty)
                        {
                            var factorValue = await context.FactorValues.FindAsync(factorValueId);



                            // TODO: если есть id, но не найден FactorValue


                            var a = worksheet.Cell(i, j).Value.ToString().Replace('.', ',');
                            if (double.TryParse(a, out var val))
                            {
                                var coef = factorValue.Values.SingleOrDefault(x =>
                                    x.FactorValueId == factorValueId && x.RiskId == riskId);
                                if (coef != null)
                                {
                                    coef.Value = val;
                                }
                                else
                                {
                                    factorValue.Values.Add(new Coefficient
                                    {
                                        FactorValueId = factorValueId,
                                        RiskId = riskId,
                                        Value = val
                                    });
                                }
                            }
                        }
                        else
                        {
                            // если дошли до пустого столбца, то прерываем for по столбцам
                            if (string.IsNullOrWhiteSpace(worksheet.Cell(1, j).Value.ToString()) &&
                                string.IsNullOrWhiteSpace(worksheet.Cell(2, j).Value.ToString()) &&
                                string.IsNullOrWhiteSpace(worksheet.Cell(3, j).Value.ToString()) &&
                                string.IsNullOrWhiteSpace(worksheet.Cell(4, j).Value.ToString()))
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // если дошли до пустой строки, то прерываем for по строкам
                    if (string.IsNullOrWhiteSpace(row.Cell(2).Value.ToString()) &&
                        string.IsNullOrWhiteSpace(row.Cell(3).Value.ToString()) &&
                        string.IsNullOrWhiteSpace(row.Cell(4).Value.ToString()) &&
                        string.IsNullOrWhiteSpace(row.Cell(5).Value.ToString()))
                    {
                        break;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
