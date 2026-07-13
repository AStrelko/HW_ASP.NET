namespace HW_06.DTOs.Participant;

/// <summary>
/// DTO з короткою інформацією про учасника.
/// Використовується для відображення списку учасників.
/// </summary>
public class ParticipantReadDTO
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
}