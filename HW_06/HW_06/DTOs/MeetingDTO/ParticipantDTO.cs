namespace HW_06.DTOs.MeetingDTO;

/// <summary>
/// DTO з інформацією про учасника зустрічі.
/// </summary>
public record ParticipantDTO
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
    /// Адреса електронної пошти,
    /// що зберігається в обліковому записі Identity.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }
}