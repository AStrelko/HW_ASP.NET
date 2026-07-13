namespace HW_06.Models;

public class MeetingParticipant
{
    public int ParticipantId { get; set; }
    public int MeetingId { get; set; }

    public Participant Participant { get; set; } = null!;
    public Meeting Meeting { get; set; } = null!;
}