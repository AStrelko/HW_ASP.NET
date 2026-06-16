namespace less_06.App.Services;


public class ProductsFilter
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string SortingKey { get; set; }
    public string SearchQuery { get; set; }
}