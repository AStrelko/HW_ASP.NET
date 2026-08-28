namespace HW_06.DTOs.Files;

/// <summary>
/// Представляє приватний файл,
/// переданий між учасниками.
/// </summary>
/// <param name="Id">
/// Унікальний ідентифікатор файлу.
/// </param>
/// <param name="OriginalFileName">
/// Оригінальне ім’я завантаженого файлу.
/// </param>
/// <param name="ContentType">
/// MIME-тип вмісту файлу.
/// </param>
/// <param name="SizeBytes">
/// Розмір файлу в байтах.
/// </param>
/// <param name="UploadedAtUtc">
/// Дата й час завантаження файлу у форматі UTC.
/// </param>
/// <param name="SenderParticipantId">
/// Ідентифікатор учасника, який надіслав файл.
/// </param>
/// <param name="SenderFullName">
/// Повне ім’я учасника, який надіслав файл.
/// </param>
/// <param name="RecipientParticipantId">
/// Ідентифікатор учасника, якому призначено файл.
/// </param>
/// <param name="RecipientFullName">
/// Повне ім’я учасника, якому призначено файл.
/// </param>
/// <param name="DownloadUrl">
/// URL-адреса для завантаження файлу.
/// </param>
public record AttachmentPrivateDTO(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc,
    int SenderParticipantId,
    string SenderFullName,
    int RecipientParticipantId,
    string RecipientFullName,
    string DownloadUrl
);