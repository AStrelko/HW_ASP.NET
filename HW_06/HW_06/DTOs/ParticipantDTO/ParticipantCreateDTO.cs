namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO для створення нового учасника.
/// </summary>
public record ParticipantCreateDTO
{
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
    /// Роль учасника.
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Список MeetingIds
    /// </summary>
    public List<int> MeetingIds { get; set; } = new();
}