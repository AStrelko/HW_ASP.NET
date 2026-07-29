namespace HW_06.Models.Files;

/// <summary>
/// Результат відкриття файлу для завантаження або перегляду.
/// </summary>
public class FileDownloadResult
{
    /// <summary>
    /// Потік із вмістом файлу.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// MIME-тип файлу.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Повне ім'я файлу разом із розширенням.
    /// </summary>
    public required string FileName { get; init; }
}