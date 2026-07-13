namespace HW_06.DTOs.Meeting;

public class ParticipantDTO
{
    public int ParticipantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
}