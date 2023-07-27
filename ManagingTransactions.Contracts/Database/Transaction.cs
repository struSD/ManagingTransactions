using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagingTransaction.Contracts.Database;
[Table("tbl_transaction")]
public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("transaction_id")]
    public int TransactionId { get; set; }
    [Column("status")]
    public string Status { get; set; }
    [Column("type")]
    public string Type { get; set; }
    [Column("client_name")]
    public string ClientName { get; set; }
    [Column("amount")]
    public decimal Amount { get; set; }
}