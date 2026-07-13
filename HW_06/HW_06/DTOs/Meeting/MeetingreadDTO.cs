namespace HW_06.DTOs.Meeting;

/// <summary>
/// DTO з короткою інформацією про зустріч.
/// Використовується для відображення списку зустрічей.
/// </summary>
public class MeetingreadDTO
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
    /// Дата та час проведення зустрічі.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Номер кімнати, у якій проводиться зустріч.
    /// </summary>
    public int? RoomNumber { get; set; }

    /// <summary>
    /// Кількість учасників зустрічі.
    /// </summary>
    public int ParticipantsCount { get; set; }
}