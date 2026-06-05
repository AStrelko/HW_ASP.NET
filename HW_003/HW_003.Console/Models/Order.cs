namespace HW_003.Console.Models;

public class Order
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; } 
    public string StreetName { get; set; } =  string.Empty;
    public decimal Price { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}