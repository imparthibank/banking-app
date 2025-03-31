using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExcelBenchmark.PlayGround
{
    [MemoryDiagnoser]
    public class ExcelBenchmark
    {
        private List<BankAccount> _data;

        [GlobalSetup]
        public void Setup()
        {
            _data = Enumerable.Range(1, 1000).Select(i => new BankAccount
            {
                AccountHolderName = $"User {i}",
                AccountNumber = $"0000{i:D6}",
                BankName = "Benchmark Bank",
                IFSCCode = "IFSC1234",
                Branch = "Main Branch",
                OpeningDate = DateTime.Today.AddDays(-i)
            }).ToList();
        }

        [Benchmark]
        public void ClosedXml_Write()
        {
            using var stream = new MemoryStream();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sheet1");
            for (int i = 0; i < _data.Count; i++)
            {
                var r = _data[i];
                ws.Cell(i + 1, 1).Value = r.AccountHolderName;
                ws.Cell(i + 1, 2).Value = r.AccountNumber;
                ws.Cell(i + 1, 3).Value = r.BankName;
                ws.Cell(i + 1, 4).Value = r.IFSCCode;
                ws.Cell(i + 1, 5).Value = r.Branch;
                ws.Cell(i + 1, 6).Value = r.OpeningDate;
            }
            workbook.SaveAs(stream);
        }

        [Benchmark]
        public void EPPlus_Write()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            for (int i = 0; i < _data.Count; i++)
            {
                var r = _data[i];
                ws.Cells[i + 1, 1].Value = r.AccountHolderName;
                ws.Cells[i + 1, 2].Value = r.AccountNumber;
                ws.Cells[i + 1, 3].Value = r.BankName;
                ws.Cells[i + 1, 4].Value = r.IFSCCode;
                ws.Cells[i + 1, 5].Value = r.Branch;
                ws.Cells[i + 1, 6].Value = r.OpeningDate;
            }
            package.SaveAs(stream);
        }

        [Benchmark]
        public void NPOI_Write()
        {
            using var stream = new MemoryStream();
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            for (int i = 0; i < _data.Count; i++)
            {
                var r = _data[i];
                var row = sheet.CreateRow(i);
                row.CreateCell(0).SetCellValue(r.AccountHolderName);
                row.CreateCell(1).SetCellValue(r.AccountNumber);
                row.CreateCell(2).SetCellValue(r.BankName);
                row.CreateCell(3).SetCellValue(r.IFSCCode);
                row.CreateCell(4).SetCellValue(r.Branch);
                row.CreateCell(5).SetCellValue(r.OpeningDate.ToShortDateString());
            }
            workbook.Write(stream);
        }

        [Benchmark]
        public void OpenXml_Write()
        {
            using var stream = new MemoryStream();
            using var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" });

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            foreach (var r in _data)
            {
                var row = new Row();
                row.Append(
                    CreateCell(r.AccountHolderName),
                    CreateCell(r.AccountNumber),
                    CreateCell(r.BankName),
                    CreateCell(r.IFSCCode),
                    CreateCell(r.Branch),
                    CreateCell(r.OpeningDate.ToShortDateString())
                );
                sheetData.Append(row);
            }
        }

        private Cell CreateCell(string text) => new Cell { DataType = CellValues.String, CellValue = new CellValue(text) };
    }
}
