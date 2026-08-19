namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO з короткою інформацією про учасника.
/// Використовується для відображення списку учасників.
/// </summary>
public record ParticipantReadDTO
{
    /// <summary>
    /// Унікальний ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

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
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Аватарка учасника
    /// </summary>
    public string? AvatarUrl { get; set; }
}