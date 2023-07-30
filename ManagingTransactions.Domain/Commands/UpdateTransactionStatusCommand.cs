using ManagingTransactions.Domain.Database;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UpdateTransactionStatus
{
    public string Status { get; set; }
}
public class UpdateTransactionStatusCommand : IRequest<bool>
{
    public int TransactionId { get; set; }
    public string Status { get; set; }
}
public class UpdateTransactionStatusCommandHandler : IRequestHandler<UpdateTransactionStatusCommand, bool>
{
    private readonly ManagingTransactionsDbContext _dbContext;

    public UpdateTransactionStatusCommandHandler(ManagingTransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<bool> Handle(UpdateTransactionStatusCommand request, CancellationToken cancellationToken)
    {
        //looking for a transaction by transaction_id
        var transaction = await _dbContext.Transactions.FindAsync(request.TransactionId);

        if (transaction == null)
        {
            return false;
        }
        //update transaction by transaction_id
        transaction.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
