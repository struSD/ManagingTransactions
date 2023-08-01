using ManagingTransaction.Contracts.Database;

using ManagingTransactions.Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ManagingTransactions.Domain.Queries;


public class TransactionFilter
{
    public List<string> Types { get; set; }
    public string Status { get; set; }
    public string ClientName { get; set; }
}
public class GetTransactionsQuery : IRequest<List<Transaction>>
{
    public TransactionFilter Filter { get; set; }
}
public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, List<Transaction>>
{
    private readonly ManagingTransactionsDbContext _dbContext;
    public GetTransactionsQueryHandler(ManagingTransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<Transaction>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        //create the SQL query
        string selectQuery = "SELECT * FROM tbl_transaction WHERE 1 = 1"; //dummy condition to start the WHERE clause

        //create a list to hold the query parameters
        var parameters = new List<Npgsql.NpgsqlParameter>();

        //filter by types of transactions, if they are specified
        if (request.Filter?.Types != null && request.Filter.Types.Any())
        {
            selectQuery += " AND type IN (" + string.Join(", ", request.Filter.Types.Select((_, i) => "@type" + i)) + ")";
            parameters.AddRange(request.Filter.Types.Select((type, i) =>
                new Npgsql.NpgsqlParameter("@type" + i, NpgsqlTypes.NpgsqlDbType.Text) { Value = type }));
        }

        //filter by the status of the transaction, if it is specified
        if (!string.IsNullOrEmpty(request.Filter?.Status))
        {
            selectQuery += " AND status = @status";
            parameters.Add(new Npgsql.NpgsqlParameter("@status", NpgsqlTypes.NpgsqlDbType.Text) { Value = request.Filter.Status });
        }

        //filter by the client's name, if it is specified
        if (!string.IsNullOrEmpty(request.Filter?.ClientName))
        {
            selectQuery += " AND client_name LIKE @client_name";
            parameters.Add(new Npgsql.NpgsqlParameter("@client_name", NpgsqlTypes.NpgsqlDbType.Text) { Value = $"%{request.Filter.ClientName}%" });
        }

        //execute the query using NpgsqlCommand
        var transactions = new List<Transaction>();

        using (var connection = _dbContext.Database.GetDbConnection())
        {
            await connection.OpenAsync(cancellationToken);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = selectQuery;
                command.Parameters.AddRange(parameters.ToArray());

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var transaction = new Transaction
                        {
                            TransactionId = reader.GetInt32(reader.GetOrdinal("transaction_id")),
                            Status = reader.GetString(reader.GetOrdinal("status")),
                            Type = reader.GetString(reader.GetOrdinal("type")),
                            ClientName = reader.GetString(reader.GetOrdinal("client_name")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("amount"))
                            
                        };

                        transactions.Add(transaction);
                    }
                }
            }
        }

        return transactions;
    }
}
