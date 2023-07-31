using ManagingTransaction.Contracts.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagingTransactions.Domain.Database;

public class ManagingTransactionsDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; init; }

    public ManagingTransactionsDbContext()
    {
    }
    public ManagingTransactionsDbContext(DbContextOptions<ManagingTransactionsDbContext> options) : base(options)
    {
    }
}