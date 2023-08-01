using ManagingTransactions.Domain.Database;

using MediatR;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using System.Threading;
using System.Threading.Tasks;

namespace ManagingTransaction.Domain.Commands;

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
        // Create the SQL query
        string updateQuery = "UPDATE tbl_transaction SET Status = @status WHERE transaction_id = @transaction_id";

        // Create Npgsql parameters for the query
        var statusParam = new NpgsqlParameter("@status", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = request.Status };
        var transactionIdParam = new NpgsqlParameter("@transaction_id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = request.TransactionId };

        // Execute the query using SqlCommand
        int rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(updateQuery, statusParam, transactionIdParam);

        return rowsAffected > 0;
    }
}
