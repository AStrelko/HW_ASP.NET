namespace HW_06.Helpers.QueryParameters;

/// <summary>
/// Параметри запиту для отримання списку учасників.
/// Містять налаштування пагінації, пошуку та сортування.
/// </summary>
public record ParticipantQueryParameters
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
    /// Пошук за прізвищем.
    /// </summary>
    public string? SearchLastName { get; set; }

    /// <summary>
    /// Поле для сортування.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Використовувати сортування у зворотному порядку.
    /// </summary>
    public bool Descending { get; set; }
}