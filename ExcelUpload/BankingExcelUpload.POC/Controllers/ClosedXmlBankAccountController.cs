using BankingExcelUpload.POC.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ClosedXmlBankAccountController : ControllerBase
{
    private List<BankAccount> SampleData() => new()
    {
        new BankAccount { AccountHolderName = "John Doe", AccountNumber = "1234567890", BankName = "Bank A", IFSCCode = "IFSC001", Branch = "Branch X", OpeningDate = DateTime.Today },
        new BankAccount { AccountHolderName = "Jane Smith", AccountNumber = "9876543210", BankName = "Bank B", IFSCCode = "IFSC002", Branch = "Branch Y", OpeningDate = DateTime.Today.AddDays(-10) },
    };

    [HttpGet("download-details")]
    public IActionResult DownloadBankAccounts()
    {
        var data = SampleData();
        MemoryStream stream = WriteAccountDeatails(data);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BankAccounts.xlsx");
    }

    private static MemoryStream WriteAccountDeatails(List<BankAccount> data)
    {
        var stream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("BankAccounts");
            var headers = new[] { "AccountHolderName", "AccountNumber", "BankName", "IFSCCode", "Branch", "OpeningDate" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontSize = 12;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                ws.Cell(i + 2, 1).Value = row.AccountHolderName;
                ws.Cell(i + 2, 2).Value = row.AccountNumber;
                ws.Cell(i + 2, 3).Value = row.BankName;
                ws.Cell(i + 2, 4).Value = row.IFSCCode;
                ws.Cell(i + 2, 5).Value = row.Branch;
                ws.Cell(i + 2, 6).Value = row.OpeningDate;
            }

            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        return stream;
    }

    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {

        var stream = new MemoryStream(); // ❌ No using here

        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Template");

            ws.Cell(1, 1).Value = "AccountHolderName";
            ws.Cell(1, 2).Value = "AccountNumber";
            ws.Cell(1, 3).Value = "BankName";
            ws.Cell(1, 4).Value = "IFSCCode";
            ws.Cell(1, 5).Value = "Branch";
            ws.Cell(1, 6).Value = "OpeningDate";

            workbook.SaveAs(stream); // Save while stream is open
        }

        stream.Position = 0; // Reset for reading
        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ClosedXmlTemplate.xlsx");
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")] // 👈 Required for file upload in Swagger
    public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
    {
        var accounts = new List<BankAccount>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            accounts.Add(new BankAccount
            {
                AccountHolderName = row.Cell(1).GetString(),
                AccountNumber = row.Cell(2).GetString(),
                BankName = row.Cell(3).GetString(),
                IFSCCode = row.Cell(4).GetString(),
                Branch = row.Cell(5).GetString(),
                OpeningDate = row.Cell(6).GetDateTime()
            });
        }

        return Ok(accounts);
    }
}

