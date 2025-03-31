using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BankingExcelUpload.POC.Models;

[ApiController]
[Route("api/[controller]")]
public class EPPlusBankAccountController : ControllerBase
{

    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Required for EPPlus

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Template");

        ws.Cells[1, 1].Value = "AccountHolderName";
        ws.Cells[1, 2].Value = "AccountNumber";
        ws.Cells[1, 3].Value = "BankName";
        ws.Cells[1, 4].Value = "IFSCCode";
        ws.Cells[1, 5].Value = "Branch";
        ws.Cells[1, 6].Value = "OpeningDate";

        var stream = new MemoryStream(package.GetAsByteArray());

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "EPPlusTemplate.xlsx");
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var accounts = new List<BankAccount>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets[0];

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            accounts.Add(new BankAccount
            {
                AccountHolderName = ws.Cells[row, 1].Text,
                AccountNumber = ws.Cells[row, 2].Text,
                BankName = ws.Cells[row, 3].Text,
                IFSCCode = ws.Cells[row, 4].Text,
                Branch = ws.Cells[row, 5].Text,
                OpeningDate = DateTime.TryParse(ws.Cells[row, 6].Text, out var date) ? date : DateTime.MinValue
            });
        }

        return Ok(accounts); // Or save to your DB
    }
}
