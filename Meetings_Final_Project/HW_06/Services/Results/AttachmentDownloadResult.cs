namespace HW_06.Services.Results;

/// <summary>
/// Містить дані документа, підготовленого для завантаження.
/// </summary>
/// <param name="Content">Потік із вмістом документа.</param>
/// <param name="ContentType">MIME-тип документа.</param>
/// <param name="OriginalFileName">
/// Оригінальне ім'я документа для клієнта.
/// </param>
public record AttachmentDownloadResult(
    Stream Content,
    string ContentType,
    string OriginalFileName
);