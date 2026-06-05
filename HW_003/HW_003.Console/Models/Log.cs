namespace HW_003.Console.Models;

public class Log
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } =  DateTime.Now;
}