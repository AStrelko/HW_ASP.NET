namespace HW_04.Core.Models;

public class Author
{
    public int Id { get; set; }
    public string AuthorFirstName { get; set; } = string.Empty;
    public string AuthorLastName { get; set; } = string.Empty;
    public List<Movie> Movies { get; set; } = new();
}