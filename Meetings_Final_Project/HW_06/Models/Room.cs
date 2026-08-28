namespace HW_06.Models;

/// <summary>
/// Модель кімнати, у якій проводяться зустрічі.
/// </summary>
public class Room
{
    /// <summary>
    /// Унікальний ідентифікатор кімнати.
    /// </summary>
    public int RoomId { get; set; }

    /// <summary>
    /// Номер кімнати.
    /// </summary>
    public int NumberRoom { get; set; }

    /// <summary>
    /// Навігаційна колекція зустрічей, що проводяться в кімнаті.
    /// </summary>
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}