using CsvHelper;
using CsvHelper.Configuration;

using ManagingTransactions.Domain.Database;

using MediatR;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ExportTransactionsQuery : IRequest<CsvFile>
{
    public string Type { get; }
    public string Status { get; }

    public ExportTransactionsQuery(string type, string status)
    {
        Type = type;
        Status = status;
    }
}
public class CsvFile
{
    public byte[] Content { get; set; }
    public string FileName { get; set; }
}
public class ExportTransactionsQueryHandler : IRequestHandler<ExportTransactionsQuery, CsvFile>
{
    private readonly ManagingTransactionsDbContext _dbContext;
    public ExportTransactionsQueryHandler(ManagingTransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<CsvFile> Handle(ExportTransactionsQuery request, CancellationToken cancellationToken)
    {
        //create and fill csv file
        var filteredTransactions = _dbContext.Transactions
            .Where(t => t.Type == request.Type && t.Status == request.Status)
            .ToList();

        if (filteredTransactions.Any())
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        csvWriter.WriteRecords(filteredTransactions);
                    }
                }
                return Task.FromResult(new CsvFile
                {
                    Content = memoryStream.ToArray(),
                    FileName = $"transactions_{request.Type}_{request.Status}.csv"
                });
            }
        }
        return Task.FromResult<CsvFile>(null);
    }
}
