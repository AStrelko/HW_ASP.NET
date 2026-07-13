namespace HW_06.DTOs.Meeting;

/// <summary>
/// DTO для створення нової зустрічі.
/// </summary>
public class MeetingcreateDTO
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
    /// Ідентифікатор кімнати, у якій відбудеться зустріч.
    /// </summary>
    public int? RoomId { get; set; }

    /// <summary>
    /// Список ідентифікаторів учасників, яких необхідно додати до зустрічі.
    /// </summary>
    public List<int> ParticipantIds { get; set; } = new();
}