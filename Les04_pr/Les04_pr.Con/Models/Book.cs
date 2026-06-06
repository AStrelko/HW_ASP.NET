namespace Les04_pr.Con.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Pages { get; set; } = 0;
    public decimal Price { get; set; } = 0;
}