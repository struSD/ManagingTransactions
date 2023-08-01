using CsvHelper;
using CsvHelper.Configuration;

using ManagingTransactions.Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ManagingTransactions.Domain.Queries;

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
    public async Task<CsvFile> Handle(ExportTransactionsQuery request, CancellationToken cancellationToken)
    {
        //create the SQL query to find transactions according to the specified parameters
        string selectQuery = "SELECT * FROM tbl_transaction WHERE type = @type AND status = @status";

        //create Npgsql parameters for the query
        var typeParam = new Npgsql.NpgsqlParameter("@type", NpgsqlTypes.NpgsqlDbType.Text) { Value = request.Type };
        var statusParam = new Npgsql.NpgsqlParameter("@status", NpgsqlTypes.NpgsqlDbType.Text) { Value = request.Status };

        using (var connection = _dbContext.Database.GetDbConnection())
        {
            await connection.OpenAsync(cancellationToken);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = selectQuery;
                command.Parameters.Add(typeParam);
                command.Parameters.Add(statusParam);

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                            {
                                using (var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)))
                                {
                                    //write the header row
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        csvWriter.WriteField(reader.GetName(i));
                                    }
                                    csvWriter.NextRecord();

                                    //write the data rows
                                    while (await reader.ReadAsync(cancellationToken))
                                    {
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            csvWriter.WriteField(reader.GetValue(i));
                                        }
                                        csvWriter.NextRecord();
                                    }
                                }
                            }

                            return new CsvFile
                            {
                                Content = memoryStream.ToArray(),
                                FileName = $"transactions_{request.Type}_{request.Status}.csv"
                            };
                        }
                    }
                }
            }
        }


        return null;
    }
}
