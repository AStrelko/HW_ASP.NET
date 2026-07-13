namespace HW_06.DTOs.Meeting;

/// <summary>
/// DTO з детальною інформацією про зустріч.
/// </summary>
public class MeetingditeylDTO
{
    /// <summary>
    /// Унікальний ідентифікатор зустрічі.
    /// </summary>
    public int MeetingId { get; set; }

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
    /// Список учасників зустрічі.
    /// </summary>
    public List<ParticipantDTO> Participants { get; set; } = new();
}