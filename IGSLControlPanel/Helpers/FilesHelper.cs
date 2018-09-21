using ClosedXML.Excel;
using DBModels.Models;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class FilesHelper
    {
        private const string PathToExcelFiles = "ExcelFiles";
        private const int StartRowAndColumnIndex = 3;

        public async Task UploadFile(IFormFile file, Tariff tariff, IGSLContext context)
        {
            if (file == null || file.Length == 0)
                return;

            var workbook = new XLWorkbook(file.OpenReadStream());
            foreach (var worksheet in workbook.Worksheets)
            {
                await ReadWorksheetData(worksheet, tariff, context);
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
                    AddWorkSheet(tariff, link.RiskFactor, workBook);
                }
                workBook.SaveAs(path);
            }
            return path;
        }

        private void AddWorkSheet(Tariff tariff, RiskFactor factor, IXLWorkbook workbook)
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
            var currentRow = StartRowAndColumnIndex;

            // заполняем строки (id | Name)
            foreach (var risk in risks)
            {
                worksheet.Cell(currentRow, 1).Value = risk.Id;
                worksheet.Cell(currentRow, 2).Value = risk.Name;
                currentRow++;
            }

            // стартовая строка для значений фактора риска = 3, т.к. по 1-й идут id риска, а на 2-й идут названия значений факторов риска
            var currentColumn = StartRowAndColumnIndex;
            // заполняем столбцы (id | Name)
            foreach (var factorValue in factor.FactorValues)
            {
                worksheet.Cell(1, currentColumn).Value = factorValue.Id;
                worksheet.Cell(2, currentColumn).Value = factorValue.Name;
                // заполняем значения
                for (var i = StartRowAndColumnIndex; i < worksheet.RowCount(); i++)
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

        private async Task ReadWorksheetData(IXLWorksheet worksheet, Tariff tariff, IGSLContext context)
        {
            var createdColIndexes = new Dictionary<int, FactorValue>();

            // получаем Id фактора риска
            Guid.TryParse(worksheet.Cell(1, 1).Value.ToString(), out var factorId);
            if(factorId == Guid.Empty) return;

            // проверим удалял ли пользователь в загружаемом файле столбцы
            await CheckDeletedColumns(worksheet, tariff, factorId, context);
            // проверим удалял ли пользователь в загружаемом файле строки
            await CheckDeletedRows(worksheet, tariff, context);

            for (var i = StartRowAndColumnIndex; i < worksheet.RowCount(); i++)
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

                    for (var j = StartRowAndColumnIndex; j < worksheet.ColumnCount(); j++)
                    {
                        // пытаемся получить id риска в первой ячейке столбца
                        Guid.TryParse(worksheet.Cell(1, j).Value.ToString(), out var factorValueId);

                        // если id получен, то это значит, что значение фактора риска было добавлено из БД,
                        // если нет, то значит юзер добавил столбец сам и надо создавать значение фактора риска
                        if (factorValueId != Guid.Empty)
                        {
                            var factorValue = await context.FactorValues.Include(x => x.Values).SingleOrDefaultAsync(x => x.Id == factorValueId);

                            // TODO: если есть id, но не найден FactorValue

                            AddCoefficient(worksheet, i, j, factorValue, riskId);
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

                            // если во второй строке есть текст, то нужно создать новое значение фактора риска
                            var newFactorValue = new FactorValue
                            {
                                Name = worksheet.Cell(2, j).Value.ToString(),
                                RiskFactorId = factorId,
                                TariffId = tariff.Id,
                                ValidFrom = DateTime.Now,
                                ValidTo = new DateTime(2100, 1, 1)
                            };

                            // если фактор риска еще не был создан(когда только перешли к новому столбцу), то создаем
                            if (!createdColIndexes.ContainsKey(j))
                            {
                                await context.FactorValues.AddAsync(newFactorValue);
                                createdColIndexes.Add(j, newFactorValue);
                            }
                            else
                            {
                                // если уже была создана запись фактора риска, то нужно к ней досоздавать коэффициенты
                                createdColIndexes.TryGetValue(j, out newFactorValue);
                            }

                            // если нормально получаем id риска из 1-й ячейки в строке, то создаем\обновляем коэффициент
                            if (Guid.TryParse(worksheet.Cell(i, 1).Value.ToString(), out var currentRiskId))
                            {
                                AddCoefficient(worksheet, i, j, newFactorValue, currentRiskId);
                            }
                            await context.SaveChangesAsync();
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

                    // риск невозможно корректно создать из файла, т.к. кроме названия нужны еще данные
                }
            }

            await context.SaveChangesAsync();
        }

        private void AddCoefficient(IXLWorksheet worksheet, int rowIndex, int colIndex, FactorValue factorValue, Guid riskId)
        {
            // сначала нужно поправить разделитель чтоб нормально распарсить
            var normalDouble = worksheet.Cell(rowIndex, colIndex).Value.ToString().Replace('.', ',');
            if (!double.TryParse(normalDouble, out var val)) return;
            // смотрим есть ли уже этот коэффициент
            var coef = factorValue.Values.SingleOrDefault(x =>
                x.FactorValueId == factorValue.Id && x.RiskId == riskId);
            // если есть - обновляем значение, если нет, то добавляем новый
            if (coef != null)
            {
                coef.Value = val;
            }
            else
            {
                factorValue.Values.Add(new Coefficient
                {
                    FactorValueId = factorValue.Id,
                    RiskId = riskId,
                    Value = val
                });
            }
        }

        private async Task CheckDeletedColumns(IXLWorksheet worksheet, Tariff tariff, Guid riskFactorId, IGSLContext context)
        {
            var colIds = new List<Guid>();
            // пытаемся поучить id значений фактора риска(столбцов) из файла
            for (var i = StartRowAndColumnIndex; i < 10000; i++)
            {
                // если не смогли распарсить, то останавливаем обработку
                if(!Guid.TryParse(worksheet.Cell(1, i).Value.ToString(), out var id)) break;
                if(!colIds.Contains(id)) colIds.Add(id);
            }

            // получаем список id значений фактора риска из базы
            var contextFactorIds = context.FactorValues.Where(x => x.RiskFactorId == riskFactorId && x.TariffId == tariff.Id)
                .Select(x => x.Id).ToList();
            // получаем разницу между списками - id, которые есть в базе, но нет в файле
            var deletedIds = contextFactorIds.Except(colIds);

            // удаляем нужные записи
            foreach (var deletedId in deletedIds)
            {
                var factorToDelete = await context.FactorValues.FindAsync(deletedId);
                if(factorToDelete == null) continue;
                factorToDelete.IsDeleted = true;
                factorToDelete.Values.ForEach(x => x.IsDeleted = true);
                await context.SaveChangesAsync();
            }
        }

        private async Task CheckDeletedRows(IXLWorksheet worksheet, Tariff tariff, IGSLContext context)
        {
            var rowIds = new List<Guid>();
            // пытаемся поучить id рисков(строк) из файла
            for (var i = StartRowAndColumnIndex; i < 10000; i++)
            {
                // если не смогли распарсить, то останавливаем обработку
                if (!Guid.TryParse(worksheet.Cell(i, 1).Value.ToString(), out var id)) break;
                if (!rowIds.Contains(id)) rowIds.Add(id);
            }

            // получаем список id рисков из базы
            var risks = new List<Guid>();
            foreach (var ruleLink in tariff.InsRuleTariffLink)
            {
                foreach (var riskLink in ruleLink.InsRule.LinksToRisks)
                {
                    if(!risks.Contains(riskLink.RiskId))
                        risks.Add(riskLink.RiskId);
                }
            }

            // получаем разницу между списками - id, которые есть в базе, но нет в файле
            var deletedIds = risks.Except(rowIds);

            // удаляем нужные записи
            foreach (var deletedId in deletedIds)
            {
                var riskToDelete = await context.Risks.FindAsync(deletedId);
                if (riskToDelete == null) continue;
                riskToDelete.IsDeleted = true;
                
                await context.SaveChangesAsync();
            }
        }
    }
}
