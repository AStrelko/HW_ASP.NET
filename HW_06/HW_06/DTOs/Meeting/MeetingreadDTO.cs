namespace HW_06.DTOs.Meeting;

public class MeetingreadDTO
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int? RoomNumber { get; set; }
    public int ParticipantsCount { get; set; }
}