namespace OnlineStoreLes02Lab.Core.Models;

public class OrderRespons
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal TotalSum { get; set; }
}