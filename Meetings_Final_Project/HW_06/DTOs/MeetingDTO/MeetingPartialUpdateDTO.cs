namespace HW_06.DTOs.MeetingDTO;

/// <summary>
/// DTO для часткового оновлення інформації про зустріч.
/// Усі поля є необов'язковими.
/// </summary>
public record MeetingPartialUpdateDTO
{
    /// <summary>
    /// Нова назва зустрічі.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Новий опис або порядок денний зустрічі.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Нова дата та час проведення зустрічі.
    /// </summary>
    public DateTime? DateTime { get; set; }

    /// <summary>
    /// Новий номер кімнати.
    /// </summary>
    public int? RoomNumber { get; set; }
}