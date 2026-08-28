namespace HW_06.DTOs.Files;

/// <summary>
/// Представляє публічний файл-вкладення зустрічі,
/// що повертається клієнту API.
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
/// <param name="DownloadUrl">
/// URL-адреса для завантаження файлу.
/// </param>
public record AttachmentPublicDTO(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc,
    string DownloadUrl
);