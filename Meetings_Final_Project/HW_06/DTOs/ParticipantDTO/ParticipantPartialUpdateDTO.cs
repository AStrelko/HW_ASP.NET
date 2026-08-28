namespace HW_06.DTOs.ParticipantDTO;

/// <summary>
/// DTO для часткового оновлення інформації про учасника.
/// Усі поля є необов'язковими.
/// </summary>
public record ParticipantPartialUpdateDTO
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
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Новий повний список ідентифікаторів зустрічей.
    /// Null означає, що зв'язки змінювати не потрібно.
    /// </summary>
    public List<int>? MeetingIds { get; set; }
}