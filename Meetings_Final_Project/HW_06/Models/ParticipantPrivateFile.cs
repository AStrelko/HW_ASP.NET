namespace HW_06.Models;

/// <summary>
/// Представляет приватный файл, переданный от одного участника другому.
/// </summary>
public class ParticipantPrivateFile
{
    /// <summary>
    /// Уникальный идентификатор приватного файла.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Оригинальное имя файла, указанное при загрузке.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Уникальное имя физического файла в хранилище.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME-тип содержимого файла.
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
    /// Идентификатор участника, который отправил файл.
    /// </summary>
    public int SenderParticipantId { get; set; }

    /// <summary>
    /// Участник, который отправил файл.
    /// </summary>
    public Participant SenderParticipant { get; set; } = null!;

    /// <summary>
    /// Идентификатор участника, которому предназначен файл.
    /// </summary>
    public int RecipientParticipantId { get; set; }

    /// <summary>
    /// Участник, которому предназначен файл.
    /// </summary>
    public Participant RecipientParticipant { get; set; } = null!;
}