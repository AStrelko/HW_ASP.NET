namespace HW_06.DTOs.Participant;

public class ParticipantCreateDTO
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Role { get; set; }
}