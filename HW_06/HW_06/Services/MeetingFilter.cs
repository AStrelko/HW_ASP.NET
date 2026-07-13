namespace HW_06.Services;

/// <summary>
/// Параметри пошуку, фільтрації, сортування та пагінації списку зустрічей.
/// </summary>
public class MeetingFilter
{
    /// <summary>
    /// Номер сторінки.
    /// За замовчуванням — 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Кількість записів на одній сторінці.
    /// За замовчуванням — 10.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Пошук зустрічей за назвою.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Поле для сортування.
    /// Можливі значення: "title", "date".
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Початкова дата для фільтрації зустрічей.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Кінцева дата для фільтрації зустрічей.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Ідентифікатор кімнати для фільтрації зустрічей.
    /// </summary>
    public int? RoomId { get; set; }
}