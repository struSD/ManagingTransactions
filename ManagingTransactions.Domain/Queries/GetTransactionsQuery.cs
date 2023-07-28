using ManagingTransaction.Contracts.Database;
using ManagingTransactions.Domain.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        IQueryable<Transaction> query = _dbContext.Transactions;

        //Filter by types of transactions, if they are specified
        if (request.Filter?.Types != null && request.Filter.Types.Any())
        {
            query = query.Where(t => request.Filter.Types.Contains(t.Type));
        }
        //Filter by the status of the transaction, if it is specified
        if (!string.IsNullOrEmpty(request.Filter?.Status))
        {
            query = query.Where(t => t.Status == request.Filter.Status);
        }
        //Ffilter by the client's name, if it is specified
        if (!string.IsNullOrEmpty(request.Filter?.ClientName))
        {
            query = query.Where(t => t.ClientName.Contains(request.Filter.ClientName));
        }
        //Return a list of transactions satisfying the specified filters
        return await query.ToListAsync(cancellationToken);
    }
}
