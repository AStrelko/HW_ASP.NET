namespace HW_06.DTOs.Meeting;

public class MeetingupdateDTO
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int? RoomId { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}