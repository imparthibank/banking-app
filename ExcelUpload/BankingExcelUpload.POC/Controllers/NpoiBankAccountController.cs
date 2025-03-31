using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BankingExcelUpload.POC.Models;

[ApiController]
[Route("api/[controller]")]
public class NpoiBankAccountController : ControllerBase
{

    [HttpGet("download-template")]
    public IActionResult DownloadTemplate()
    {
        using var tempStream = new MemoryStream(); // This can be disposed

        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Template");

        var header = sheet.CreateRow(0);
        header.CreateCell(0).SetCellValue("AccountHolderName");
        header.CreateCell(1).SetCellValue("AccountNumber");
        header.CreateCell(2).SetCellValue("BankName");
        header.CreateCell(3).SetCellValue("IFSCCode");
        header.CreateCell(4).SetCellValue("Branch");
        header.CreateCell(5).SetCellValue("OpeningDate");

        workbook.Write(tempStream); // NPOI closes this stream
        var bytes = tempStream.ToArray(); // Get byte array before it's disposed

        var finalStream = new MemoryStream(bytes); // New stream for ASP.NET
        return File(finalStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "NpoiTemplate.xlsx");

    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
    {
        var accounts = new List<BankAccount>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        for (int row = 1; row <= sheet.LastRowNum; row++) // Skip header (row 0)
        {
            var currentRow = sheet.GetRow(row);
            if (currentRow == null) continue;

            accounts.Add(new BankAccount
            {
                AccountHolderName = currentRow.GetCell(0)?.ToString(),
                AccountNumber = currentRow.GetCell(1)?.ToString(),
                BankName = currentRow.GetCell(2)?.ToString(),
                IFSCCode = currentRow.GetCell(3)?.ToString(),
                Branch = currentRow.GetCell(4)?.ToString(),
                OpeningDate = DateTime.TryParse(currentRow.GetCell(5)?.ToString(), out var date)
                                ? date
                                : DateTime.MinValue
            });
        }

        return Ok(accounts);
    }
}
