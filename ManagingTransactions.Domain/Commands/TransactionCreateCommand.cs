using ManagingTransaction.Contracts.Database;

using ManagingTransactions.Domain.Database;

using MediatR;

using System.Threading;
using System.Threading.Tasks;
public class TransactionCreateCommand : IRequest<TransactionCreateCommandResult>
{
    public int TransactionId { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public string ClientName { get; set; }
    public decimal Amount { get; set; }
}
public class TransactionCreateCommandResult
{
    public int TransactionId { get; set; }
}
public class TransactionCreateCommandHandler : IRequestHandler<TransactionCreateCommand, TransactionCreateCommandResult>
{
    private readonly ManagingTransactionsDbContext _dbContext;
    public TransactionCreateCommandHandler(ManagingTransactionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<TransactionCreateCommandResult> Handle(TransactionCreateCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction
        {
            TransactionId = request.TransactionId,
            Status = request.Status,
            Type = request.Type,
            ClientName = request.ClientName,
            Amount = request.Amount
        };
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TransactionCreateCommandResult
        {
            TransactionId = transaction.TransactionId
        };
    }
}