namespace HW_06.DTOs.Participant;

/// <summary>
/// DTO для часткового оновлення інформації про учасника.
/// Усі поля є необов'язковими.
/// </summary>
public class ParticipantPartialUpdateDTO
{
    /// <summary>
    /// Нове ім'я учасника.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Нове прізвище учасника.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Нова адреса електронної пошти учасника.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Нова роль учасника під час зустрічі.
    /// </summary>
    public string? Role { get; set; }
}