namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO для повного оновлення інформації про учасника.
/// </summary>
public record ParticipantUpdateDTO
{
    /// <summary>
    /// Ім'я учасника.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Повний список ідентифікаторів зустрічей учасника.
    /// </summary>
    public List<int> MeetingIds { get; set; } = [];
}