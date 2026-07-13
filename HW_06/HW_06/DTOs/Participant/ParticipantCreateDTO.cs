namespace HW_06.DTOs.Participant;

/// <summary>
/// DTO для створення нового учасника.
/// </summary>
public class ParticipantCreateDTO
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
}