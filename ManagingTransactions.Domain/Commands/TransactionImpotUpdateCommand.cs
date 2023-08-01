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
            //searches for a transaction with the specified TransactionId in the DB, and the query result is stored in the existingTransaction variable
            var existingTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionData.TransactionId);

            if (existingTransaction != null)
            {
                //updates the status if the transaction exists
                existingTransaction.Status = transactionData.Status;
            }
            else
            {
                //if it doesn't exist, it creates it
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
