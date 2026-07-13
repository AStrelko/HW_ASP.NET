namespace HW_06.Models;

public class Room
{
    public int RoomId { get; set; }
    public int NumberRoom { get; set; }
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>(); 
}