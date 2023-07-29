using ManagingTransaction.Domain.Commands;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
[Route("api/transactions")]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("upload"),Authorize]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var transactions = new List<TransactionData>();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        transactions.Add(new TransactionData
                        {
                            TransactionId = int.Parse(worksheet.Cells[row, 1].Value.ToString()),
                            Status = worksheet.Cells[row, 2].Value.ToString(),
                            Type = worksheet.Cells[row, 3].Value.ToString(),
                            ClientName = worksheet.Cells[row, 4].Value.ToString(),
                            Amount = decimal.Parse(worksheet.Cells[row, 5].Value.ToString(), CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
        }
        if (transactions.Any())
        {
            await _mediator.Send(new TransactionImpotUpdateCommand { Transactions = transactions });
            return Ok("Data from Excel uploaded and processed.");
        }
        return BadRequest("No data found in the Excel file.");
    }
    [HttpGet("export"),Authorize]
    public async Task<IActionResult> ExportTransactions(string type, string status)
    {
        var csvFile = await _mediator.Send(new ExportTransactionsQuery(type, status));

        if (csvFile != null)
        {
            return File(csvFile.Content, "text/csv", csvFile.FileName);
        }
        return BadRequest("No data found for the specified filters.");
    }
    [HttpGet,Authorize]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilter filter)
    {
        var transactions = await _mediator.Send(new GetTransactionsQuery { Filter = filter });
        return Ok(transactions);
    }
    [HttpPut("{id}/status"),Authorize]
    public async Task<IActionResult> UpdateTransactionStatus(int id, [FromBody] UpdateTransactionStatus model)
    {
        var result = await _mediator.Send(new UpdateTransactionStatusCommand { TransactionId = id, Status = model.Status });

        if (result)
        {
            return NoContent();
        }

        return NotFound();
    }
}

