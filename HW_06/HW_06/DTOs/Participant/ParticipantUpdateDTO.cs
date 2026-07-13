namespace HW_06.DTOs.Participant;

public class ParticipantUpdateDTO
{
    public int ParticipantId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Role { get; set; }
}