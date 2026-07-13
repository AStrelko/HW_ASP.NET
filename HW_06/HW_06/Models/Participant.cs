namespace HW_06.Models;

/// <summary>
/// Модель учасника зустрічі.
/// </summary>
public class Participant
{
    /// <summary>
    /// Унікальний ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Ім'я учасника.
    /// </summary>
    public string FirstName { get; set; } = "";

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; set; } = "";

    /// <summary>
    /// Адреса електронної пошти учасника.
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Роль учасника під час зустрічі.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Колекція зв'язків між учасником та зустрічами.
    /// </summary>
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
        = new List<MeetingParticipant>();
}