namespace less_06.App.Services;


public class ProductsFilter
{
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;

    public string? SortingKey { get; set; }
    public string? SearchQuery { get; set; }
}