namespace HW_06.DTOs.Meeting;

public class MeetingditeylDTO
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int? RoomNumber { get; set; }
    public List<ParticipantDTO> Participants { get; set; } = new();
}