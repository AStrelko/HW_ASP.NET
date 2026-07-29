namespace HW_06.DTOs.Participants;

/// <summary>
/// DTO для короткого відображення учасника разом з аватаром.
/// </summary>
public record ParticipantAvatarDTO
{
    /// <summary>
    /// Унікальний ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; init; }

    /// <summary>
    /// Ім’я учасника.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Серверне ім’я файлу аватара разом із розширенням.
    /// </summary>
    public string? AvatarFileName { get; init; }

    /// <summary>
    /// URL для отримання аватара.
    /// </summary>
    public string? AvatarUrl { get; init; }
}