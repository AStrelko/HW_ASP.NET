namespace HW_06.DTOs.Meeting;

/// <summary>
/// DTO з інформацією про учасника зустрічі.
/// </summary>
public class ParticipantDTO
{
    /// <summary>
    /// Унікальний ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Ім'я учасника.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Адреса електронної пошти учасника.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Роль учасника під час зустрічі.
    /// </summary>
    public string? Role { get; set; }
}