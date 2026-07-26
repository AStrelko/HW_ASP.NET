namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO для повного оновлення інформації про учасника.
/// </summary>
public record ParticipantUpdateDTO
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
    /// Роль учасника.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Повний список ідентифікаторів зустрічей учасника.
    /// </summary>
    public List<int> MeetingIds { get; set; } = new();
}