namespace HW_06.Helpers.QueryParameters;

/// <summary>
/// Фільтр для пошуку зустрічей.
/// Використовується для відбору записів за певними умовами.
/// </summary>
public class MeetingFilter
{
    /// <summary>
    /// Початкова дата проведення зустрічей.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Кінцева дата проведення зустрічей.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Номер кімнати.
    /// </summary>
    public int? RoomNumber { get; set; }
}