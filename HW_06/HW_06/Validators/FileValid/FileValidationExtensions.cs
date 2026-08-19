using Microsoft.AspNetCore.Http;

namespace HW_06.Validators.FileValid;

/// <summary>
/// Містить спільні методи перевірки файлів.
/// </summary>
public static class FileValidationExtensions
{
    /// <summary>
    /// Перевіряє, чи файл передано
    /// та чи містить він дані.
    /// </summary>
    public static string? ValidateRequiredFile(
        IFormFile? file)
    {
        if (file is null)
        {
            return "Файл не було передано.";
        }

        if (file.Length == 0)
        {
            return "Не можна завантажити порожній файл.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє максимальний розмір файлу.
    /// </summary>
    public static string? ValidateFileSize(
        IFormFile file,
        long maxFileSize)
    {
        if (file.Length > maxFileSize)
        {
            return
                $"Розмір файлу не повинен перевищувати " +
                $"{maxFileSize / 1024 / 1024} МБ.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє ім'я файлу.
    /// </summary>
    public static string? ValidateFileName(
        IFormFile file,
        int maxLength = 255)
    {
        var fileName =
            Path.GetFileName(file.FileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Ім’я файлу відсутнє.";
        }

        if (fileName.Length > maxLength)
        {
            return "Ім’я файлу занадто довге.";
        }

        return null;
    }

    /// <summary>
    /// Повертає нормалізоване розширення файлу.
    /// </summary>
    public static string GetNormalizedExtension(
        IFormFile file)
    {
        return Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Перевіряє, чи дозволене розширення файлу.
    /// </summary>
    public static bool HasAllowedExtension(
        IFormFile file,
        IEnumerable<string> allowedExtensions)
    {
        var extension =
            GetNormalizedExtension(file);

        return !string.IsNullOrWhiteSpace(extension) &&
               allowedExtensions.Contains(
                   extension,
                   StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Перевіряє відповідність MIME-типу
    /// розширенню файлу.
    /// </summary>
    public static bool HasValidContentType(
        IFormFile file,
        IReadOnlyDictionary<string, string> allowedContentTypes)
    {
        var extension =
            GetNormalizedExtension(file);

        return allowedContentTypes.TryGetValue(
                   extension,
                   out var expectedContentType)
               &&
               string.Equals(
                   file.ContentType,
                   expectedContentType,
                   StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Перевіряє, чи починається файл
    /// із заданої послідовності байтів.
    /// </summary>
    public static async Task<bool> StartsWithAsync(
        Stream stream,
        byte[] expectedBytes)
    {
        var actualBytes =
            new byte[expectedBytes.Length];

        var bytesRead =
            await stream.ReadAsync(
                actualBytes.AsMemory(
                    0,
                    actualBytes.Length));

        return bytesRead == expectedBytes.Length &&
               actualBytes.SequenceEqual(expectedBytes);
    }

    /// <summary>
    /// Перевіряє, чи містить потік
    /// нульовий байт.
    /// </summary>
    public static bool ContainsNullByte(
        Stream stream)
    {
        const int bufferSize = 4096;

        var buffer =
            new byte[bufferSize];

        int bytesRead;

        while ((bytesRead = stream.Read(
                   buffer,
                   0,
                   buffer.Length)) > 0)
        {
            for (var index = 0;
                 index < bytesRead;
                 index++)
            {
                if (buffer[index] == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Перевіряє сигнатуру документа.
    /// </summary>
    public static async Task<bool> HasValidDocumentSignatureAsync(
        IFormFile file,
        string extension)
    {
        await using var stream =
            file.OpenReadStream();

        return extension switch
        {
            ".pdf" =>
                await StartsWithAsync(
                    stream,
                    new byte[]
                    {
                        0x25, 0x50, 0x44, 0x46
                    }),

            ".docx" =>
                await StartsWithAsync(
                    stream,
                    new byte[]
                    {
                        0x50, 0x4B, 0x03, 0x04
                    }),

            ".txt" =>
                !ContainsNullByte(stream),

            _ => false
        };
    }
}