namespace HW_06.Models;

public class Meeting
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime DateTime { get; set; }
    
    public int? RoomId { get; set; }
    
    public Room? Room { get; set; } 
    
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
        = new List<MeetingParticipant>();
}