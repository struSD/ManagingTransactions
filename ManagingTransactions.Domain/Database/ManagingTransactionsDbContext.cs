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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ManagingTransaction;Username=postgres;Password=261095;");
    }
}