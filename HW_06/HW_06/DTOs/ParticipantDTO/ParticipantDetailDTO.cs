using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.Files;
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
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Аватарка учасника
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Список зустрічей учасника.
    /// </summary>
    public List<MeetingReadDTO> Meetings { get; set; } = new();
    
    /// <summary>
    /// Приватні файли, надіслані учасником.
    /// </summary>
    public IReadOnlyCollection<AttachmentPrivateDTO> SentPrivateFiles { get; set; }
        = [];

    /// <summary>
    /// Приватні файли, отримані учасником.
    /// </summary>
    public IReadOnlyCollection<AttachmentPrivateDTO> ReceivedPrivateFiles { get; set; }
        = [];
}