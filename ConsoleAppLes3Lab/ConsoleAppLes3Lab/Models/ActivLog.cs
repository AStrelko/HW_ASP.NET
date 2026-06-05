namespace ConsoleAppLes3Lab.Models;

public class ActivLog
{
    public int Id { get; set; }
    public DateTime DateAt { get; set; } = DateTime.Now;
    public string Message { get; set; } = string.Empty;
}