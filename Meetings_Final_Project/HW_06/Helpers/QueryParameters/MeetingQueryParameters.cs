namespace HW_06.Helpers.QueryParameters;

/// <summary>
/// Параметри запиту для отримання списку зустрічей.
/// Містять налаштування пагінації, пошуку та сортування.
/// </summary>
public record MeetingQueryParameters
{
    /// <summary>
    /// Номер сторінки.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Кількість записів на сторінці.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Пошук за назвою зустрічі.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Поле сортування.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Сортувати у зворотному порядку.
    /// </summary>
    public bool Descending { get; set; }
}