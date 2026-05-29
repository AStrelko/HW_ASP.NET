namespace DictionaryApp.Core.Models;

public class DictionaryItem
{
    public int Id { get; set; }
    public string Name { get; set; }= string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string DescriptionLanguage { get; set; }= string.Empty;
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   
}