namespace HW_06.Storage.Configurations;

/// <summary>
/// Містить налаштування файлового сховища
/// та обмеження для завантажуваних файлів.
/// </summary>
public class FileStorageOptions
{
    /// <summary>
    /// Назва секції конфігурації
    /// файлового сховища.
    /// </summary>
    public const string SectionName =
        "FileStorage";

    /// <summary>
    /// Максимальний розмір аватара
    /// у мегабайтах.
    /// </summary>
    public int MaxAvatarSizeMb { get; set; } =
        5;

    /// <summary>
    /// Максимальний розмір
    /// публічного документа
    /// у мегабайтах.
    /// </summary>
    public int MaxPublicDocumentSizeMb { get; set; } =
        10;

    /// <summary>
    /// Максимальний розмір
    /// приватного документа
    /// у мегабайтах.
    /// </summary>
    public int MaxPrivateDocumentSizeMb { get; set; } =
        10;

    /// <summary>
    /// Максимальний розмір аватара
    /// у байтах.
    /// </summary>
    public long MaxAvatarSizeBytes =>
        MaxAvatarSizeMb * 1024L * 1024L;

    /// <summary>
    /// Максимальний розмір
    /// публічного документа
    /// у байтах.
    /// </summary>
    public long MaxPublicDocumentSizeBytes =>
        MaxPublicDocumentSizeMb * 1024L * 1024L;

    /// <summary>
    /// Максимальний розмір
    /// приватного документа
    /// у байтах.
    /// </summary>
    public long MaxPrivateDocumentSizeBytes =>
        MaxPrivateDocumentSizeMb * 1024L * 1024L;
}