namespace HW_06.Models;

/// <summary>
/// Представляет файл-вложение, прикреплённый к встрече.
/// Содержит метаданные файла и информацию о его расположении
/// в файловом хранилище.
/// </summary>
public class MeetingAttachment
{
    /// <summary>
    /// Уникальный идентификатор вложения.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Оригинальное имя файла, переданное пользователем.
    /// Используется для отображения и скачивания файла.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Уникальное имя файла, под которым он хранится на диске.
    /// Обычно формируется на основе значения <see cref="Guid"/>.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME-тип содержимого файла.
    /// Например: application/pdf или image/png.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Дата и время загрузки файла в формате UTC.
    /// </summary>
    public DateTime UploadedAtUtc { get; set; }

    /// <summary>
    /// Идентификатор встречи, к которой прикреплён файл.
    /// </summary>
    public int MeetingId { get; set; }

    /// <summary>
    /// Встреча, к которой относится данное вложение.
    /// </summary>
    public Meeting Meeting { get; set; } = null!;
}