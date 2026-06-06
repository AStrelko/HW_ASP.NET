namespace HW_04.Core.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Rating { get; set; } = 0;
    public int Year { get; set; }
    public int AuthorId { get; set; }
    public Author? Author { get; set; }
}