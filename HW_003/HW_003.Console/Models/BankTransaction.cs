namespace HW_003.Console.Models;

public class BankTransaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } =  DateTime.Now;
}