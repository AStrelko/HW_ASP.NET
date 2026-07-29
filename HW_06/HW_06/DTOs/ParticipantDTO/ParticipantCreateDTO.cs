using Microsoft.AspNetCore.Http;

namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// Дані для створення нового учасника.
/// </summary>
public record ParticipantCreateDTO
{
    /// <summary>
    /// Ім’я учасника.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Електронна адреса учасника.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Роль учасника.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Ідентифікатори зустрічей учасника.
    /// </summary>
    public List<int> MeetingIds { get; init; } = [];

    /// <summary>
    /// Необов’язковий файл аватара.
    /// </summary>
    public IFormFile? Avatar { get; init; }
}