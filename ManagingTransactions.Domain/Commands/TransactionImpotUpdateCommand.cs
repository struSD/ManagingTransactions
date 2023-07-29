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
            var existingTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionData.TransactionId);

            if (existingTransaction != null)
            {
                existingTransaction.Status = transactionData.Status;
            }
            else
            {
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
        return Unit.Value;
    }
}
