using Microsoft.AspNetCore.Http;

namespace HW_06.Features.Common.Files;

/// <summary>
/// Містить спільні допоміжні методи
/// для перевірки завантажуваних файлів.
/// </summary>
public static class FileValidationExtensions
{
    /// <summary>
    /// Перевіряє, що файл передано
    /// та він не є порожнім.
    /// </summary>
    /// <param name="file">
    /// Файл для перевірки.
    /// </param>
    /// <returns>
    /// Повідомлення про помилку або
    /// <see langword="null"/>, якщо файл є коректним.
    /// </returns>
    public static string? ValidateRequiredFile(
        IFormFile? file)
    {
        if (file is null)
        {
            return "Файл є обов’язковим.";
        }

        if (file.Length <= 0)
        {
            return "Файл не може бути порожнім.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє, що розмір файлу
    /// не перевищує допустиме значення.
    /// </summary>
    /// <param name="file">
    /// Файл для перевірки.
    /// </param>
    /// <param name="maxBytes">
    /// Максимальний допустимий розмір
    /// файлу в байтах.
    /// </param>
    /// <returns>
    /// Повідомлення про помилку або
    /// <see langword="null"/>, якщо розмір є допустимим.
    /// </returns>
    public static string? ValidateFileSize(
        IFormFile file,
        long maxBytes)
    {
        if (file.Length > maxBytes)
        {
            return
                $"Розмір файлу не може перевищувати " +
                $"{maxBytes / 1024 / 1024} МБ.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє коректність імені файлу.
    /// </summary>
    /// <param name="file">
    /// Файл для перевірки.
    /// </param>
    /// <returns>
    /// Повідомлення про помилку або
    /// <see langword="null"/>, якщо ім’я є коректним.
    /// </returns>
    public static string? ValidateFileName(
        IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(
                file.FileName))
        {
            return "Ім’я файлу є обов’язковим.";
        }

        var fileName =
            Path.GetFileName(
                file.FileName);

        if (!string.Equals(
                fileName,
                file.FileName,
                StringComparison.Ordinal))
        {
            return "Некоректне ім’я файлу.";
        }

        return null;
    }

    /// <summary>
    /// Повертає розширення файлу
    /// у нижньому регістрі.
    /// </summary>
    /// <param name="file">
    /// Файл, розширення якого потрібно отримати.
    /// </param>
    public static string GetNormalizedExtension(
        IFormFile file)
    {
        return Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Перевіряє, чи має файл
    /// дозволене розширення.
    /// </summary>
    /// <param name="file">
    /// Файл для перевірки.
    /// </param>
    /// <param name="allowedExtensions">
    /// Колекція дозволених розширень.
    /// </param>
    public static bool HasAllowedExtension(
        IFormFile file,
        IEnumerable<string> allowedExtensions)
    {
        var extension =
            GetNormalizedExtension(file);

        return allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
/// Перевіряє, чи відповідає MIME-тип файлу
/// його розширенню.
/// </summary>
/// <param name="file">
/// Файл для перевірки.
/// </param>
/// <param name="allowedContentTypes">
/// Відповідність розширень
/// дозволеним MIME-типам.
/// </param>
/// <returns>
/// <see langword="true"/>,
/// якщо MIME-тип відповідає розширенню;
/// інакше — <see langword="false"/>.
/// </returns>
public static bool HasValidContentType(
    IFormFile file,
    IReadOnlyDictionary<string, string> allowedContentTypes)
{
    var extension =
        GetNormalizedExtension(file);

    if (!allowedContentTypes.TryGetValue(
            extension,
            out var expectedContentType))
    {
        return false;
    }

    return string.Equals(
        file.ContentType,
        expectedContentType,
        StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Перевіряє сигнатуру документа
/// відповідно до його розширення.
/// </summary>
/// <param name="file">
/// Файл для перевірки.
/// </param>
/// <param name="extension">
/// Нормалізоване розширення файлу.
/// </param>
/// <returns>
/// <see langword="true"/>,
/// якщо сигнатура файлу є коректною;
/// інакше — <see langword="false"/>.
/// </returns>
public static async Task<bool> HasValidDocumentSignatureAsync(
    IFormFile file,
    string extension)
{
    await using var stream =
        file.OpenReadStream();

    switch (extension)
    {
        case ".pdf":
        {
            var signature =
                new byte[5];

            var bytesRead =
                await stream.ReadAsync(
                    signature.AsMemory(
                        0,
                        signature.Length));

            return bytesRead == signature.Length &&
                   signature.SequenceEqual(
                       "%PDF-"u8.ToArray());
        }

        case ".docx":
        {
            var signature =
                new byte[4];

            var bytesRead =
                await stream.ReadAsync(
                    signature.AsMemory(
                        0,
                        signature.Length));

            return bytesRead == signature.Length &&
                   signature.SequenceEqual(
                       new byte[]
                       {
                           0x50,
                           0x4B,
                           0x03,
                           0x04
                       });
        }

        case ".txt":
            return true;

        default:
            return false;
    }
}
}