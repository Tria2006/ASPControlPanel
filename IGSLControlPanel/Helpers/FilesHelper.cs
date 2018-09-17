using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DBModels.Models;
using Microsoft.AspNetCore.Http;

namespace IGSLControlPanel.Helpers
{
    public class FilesHelper
    {
        private const string PathToExcelFiles = "ExcelFiles";
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet"},
                {".csv", "text/csv"}
            };
        }

        public async Task UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(), PathToExcelFiles,
                file.Name);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task DownloadFile(string fileName)
        {
            if (fileName == null)
                return;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                PathToExcelFiles, fileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            //return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        public string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        public string CreateExcel(Tariff tariff, RiskFactor factor)
        {
            var fileName = "tempFile.xlsx";
            var path = Path.Combine(
                Directory.GetCurrentDirectory(), PathToExcelFiles,
                fileName);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Коэффициенты");

                var risks = new List<Risk>();
                foreach (var ruleLink in tariff.InsRuleTariffLink)
                {
                    foreach (var riskLink in ruleLink.InsRule.LinksToRisks)
                    {
                        if (risks.TrueForAll(x => x.Id != riskLink.Risk.Id))
                            risks.Add(riskLink.Risk);
                    }
                }

                var currentRow = 3;
                var currentColumn = 1;

                foreach (var risk in risks)
                {
                    worksheet.Cell(currentRow, currentColumn).Value = risk.Id;
                    worksheet.Cell(currentRow, currentColumn + 1).Value = risk.Name;
                    currentRow++;
                }

                currentColumn = 3;
                foreach (var factorValue in factor.FactorValues)
                {
                    worksheet.Cell(1, currentColumn).Value = factorValue.Id;
                    worksheet.Cell(2, currentColumn).Value = factorValue.Name;
                    currentColumn++;
                }

                worksheet.Rows("1").Collapse();
                worksheet.Columns("1").Collapse();
                workbook.SaveAs(path);
            }

            return path;
        }
    }
}
