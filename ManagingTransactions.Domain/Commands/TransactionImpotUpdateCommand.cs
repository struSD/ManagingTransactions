using ManagingTransaction.Contracts.Database;

using ManagingTransactions.Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagingTransaction.Domain.Commands;

public class TransactionData
{
    public int TransactionId { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string ClientName { get; set; }
    public decimal Amount { get; set; }
}
public class TransactionImpotUpdateCommand : IRequest
{
    public List<TransactionData> Transactions { get; set; }
}

public class TransactionImpotUpdateCommandHandler : IRequestHandler<TransactionImpotUpdateCommand>
{
    private readonly ManagingTransactionsDbContext _dbContext;

    public TransactionImpotUpdateCommandHandler(ManagingTransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(TransactionImpotUpdateCommand request, CancellationToken cancellationToken)
    {
        foreach (var transactionData in request.Transactions)
        {
            //create the SQL query to search for a transaction by TransactionId
            string selectQuery = "SELECT * FROM tbl_transaction WHERE transaction_id = @transaction_id";

            //create the Npgsql parameter for the query
            var transactionIdParam = new Npgsql.NpgsqlParameter("@transaction_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = transactionData.TransactionId };

            //execute the query using NpgsqlCommand
            var existingTransaction = await _dbContext.Transactions.FromSqlRaw(selectQuery, transactionIdParam).FirstOrDefaultAsync();

            if (existingTransaction != null)
            {
                //update the status if the transaction exists
                existingTransaction.Status = transactionData.Status;
            }
            else
            {
                //if it doesnt exist create a new transaction
                var newTransaction = new Transaction
                {
                    TransactionId = transactionData.TransactionId,
                    Status = transactionData.Status,
                    Type = transactionData.Type,
                    ClientName = transactionData.ClientName,
                    Amount = transactionData.Amount
                };

                _dbContext.Transactions.Add(newTransaction);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        //returns a Unit.Value indicating the successful completion of processing the request without returning any additional data or results.
        return Unit.Value;
    }
}
