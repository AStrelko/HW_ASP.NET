using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.Files;

namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO з детальною інформацією про учасника.
/// </summary>
public record ParticipantDetailDTO
{
    /// <summary>
    /// Ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Ім'я.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Прізвище.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Електронна пошта.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Роль учасника.
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Аватарка учасника
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Список зустрічей учасника.
    /// </summary>
    public List<MeetingReadDTO> Meetings { get; set; } = new();
    
    /// <summary>
    /// прикреплені до участника файли
    /// </summary>
    public List<FileReadDTO> Files { get; set; } = [];
}