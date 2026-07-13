namespace HW_06.DTOs.Meeting;

public class MeetingpartialUpdateDTO
{
    
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DateTime { get; set; }

    public int? RoomId { get; set; }
}