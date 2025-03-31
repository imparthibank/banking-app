using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using BankingExcelUpload.POC.Models;

[ApiController]
[Route("api/[controller]")]
public class OpenXmlBankAccountController : ControllerBase
{

    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {
        var stream = new MemoryStream();

        using (var spreadsheet = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Template"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            var row = new Row();
            row.Append(
                CreateCell("AccountHolderName"),
                CreateCell("AccountNumber"),
                CreateCell("BankName"),
                CreateCell("IFSCCode"),
                CreateCell("Branch"),
                CreateCell("OpeningDate")
            );
            sheetData.Append(row);
        }

        stream.Position = 0;

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "OpenXmlTemplate.xlsx");
    }


    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
    {
        var accounts = new List<BankAccount>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var spreadsheet = SpreadsheetDocument.Open(stream, false);
        var sheet = spreadsheet.WorkbookPart.Workbook.Sheets.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)spreadsheet.WorkbookPart.GetPartById(sheet.Id);
        var rows = worksheetPart.Worksheet.Descendants<Row>().Skip(1); // Skip header

        foreach (var row in rows)
        {
            var cells = row.Elements<Cell>().ToList();

            accounts.Add(new BankAccount
            {
                AccountHolderName = GetCellValue(spreadsheet, cells.ElementAtOrDefault(0)),
                AccountNumber = GetCellValue(spreadsheet, cells.ElementAtOrDefault(1)),
                BankName = GetCellValue(spreadsheet, cells.ElementAtOrDefault(2)),
                IFSCCode = GetCellValue(spreadsheet, cells.ElementAtOrDefault(3)),
                Branch = GetCellValue(spreadsheet, cells.ElementAtOrDefault(4)),
                OpeningDate = DateTime.TryParse(GetCellValue(spreadsheet, cells.ElementAtOrDefault(5)), out var date)
                                ? date
                                : DateTime.MinValue
            });
        }

        return Ok(accounts);
    }

    private Cell CreateCell(string text) =>
        new Cell { DataType = CellValues.String, CellValue = new CellValue(text) };

    private string GetCellValue(SpreadsheetDocument doc, Cell cell)
    {
        if (cell == null || cell.CellValue == null) return string.Empty;

        var value = cell.CellValue.InnerText;
        if (cell.DataType != null && cell.DataType == CellValues.SharedString)
        {
            return doc.WorkbookPart.SharedStringTablePart.SharedStringTable
                .Elements<SharedStringItem>().ElementAt(int.Parse(value)).InnerText;
        }
        return value;
    }
}
