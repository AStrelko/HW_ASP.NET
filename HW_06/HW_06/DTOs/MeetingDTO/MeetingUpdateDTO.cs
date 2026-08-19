namespace HW_06.DTOs.MeetingDTO;

/// <summary>
/// DTO для повного оновлення інформації про зустріч.
/// Усі основні поля є обов'язковими.
/// </summary>
public record MeetingUpdateDTO
{
    /// <summary>
    /// Назва зустрічі.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Опис або порядок денний зустрічі.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата та час проведення зустрічі.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Номер кімнати, у якій проводиться зустріч.
    /// </summary>
    public int? RoomNumber { get; set; }

    /// <summary>
    /// Список ідентифікаторів учасників зустрічі.
    /// </summary>
    public List<int> ParticipantIds { get; set; } = new();
}