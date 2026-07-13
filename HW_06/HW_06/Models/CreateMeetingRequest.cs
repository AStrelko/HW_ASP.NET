namespace HW_06.Models;

public class CreateMeetingRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public int? Days { get; set; }

    public int? Count { get; set; }
    
    public int? RoomId { get; set; }
    
    public List<int> ParticipantIds { get; set; } = new();
}