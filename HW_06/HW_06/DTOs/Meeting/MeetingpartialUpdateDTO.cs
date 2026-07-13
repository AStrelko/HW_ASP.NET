namespace HW_06.DTOs.Meeting;

/// <summary>
/// DTO для часткового оновлення інформації про зустріч.
/// Усі поля є необов'язковими.
/// </summary>
public class MeetingpartialUpdateDTO
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
    /// Новий ідентифікатор кімнати.
    /// </summary>
    public int? RoomId { get; set; }
}