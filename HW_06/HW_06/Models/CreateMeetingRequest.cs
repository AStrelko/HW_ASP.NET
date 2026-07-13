namespace HW_06.Models;

/// <summary>
/// Модель запиту для створення нової зустрічі.
/// </summary>
public class CreateMeetingRequest
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
    /// Кількість днів для створення повторюваних зустрічей.
    /// </summary>
    public int? Days { get; set; }

    /// <summary>
    /// Кількість зустрічей, які потрібно створити.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Ідентифікатор кімнати, у якій відбудеться зустріч.
    /// </summary>
    public int? RoomId { get; set; }

    /// <summary>
    /// Список ідентифікаторів учасників зустрічі.
    /// </summary>
    public List<int> ParticipantIds { get; set; } = new();
}