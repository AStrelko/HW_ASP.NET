using HW_06.Validators.ResultsValid;
using Microsoft.AspNetCore.Http;

namespace HW_06.Validators.FileValid;

/// <summary>
/// Перевіряє приватні документи учасників
/// перед їх збереженням.
/// </summary>
public class PrivateDocumentValidator
{
    /// <summary>
    /// Максимально допустимий розмір файлу — 10 МБ.
    /// </summary>
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 МБ

    /// <summary>
    /// Розширення документів,
    /// дозволених для завантаження.
    /// </summary>
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".docx",
            ".txt"
        };

    /// <summary>
    /// Відповідність дозволених розширень
    /// очікуваним MIME-типам.
    /// </summary>
    private static readonly Dictionary<string, string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",

            [".docx"] =
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            [".txt"] = "text/plain"
        };

    /// <summary>
    /// Перевіряє розмір, ім’я, розширення,
    /// MIME-тип і сигнатуру приватного файлу.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно перевірити.
    /// </param>
    /// <returns>
    /// Результат валідації з колекцією знайдених помилок.
    /// </returns>
    public async Task<ValidationResult> ValidateAsync(IFormFile? file)
    {
        var result = new ValidationResult();

        // Перевіряє наявність файлу.
        if (file is null)
        {
            result.AddError(
                "File",
                "Файл не було передано.");

            return result;
        }

        // Перевіряє, що файл не порожній.
        if (file.Length == 0)
        {
            result.AddError(
                "File",
                "Не можна завантажити порожній файл.");
        }

        // Перевіряє допустимий розмір файлу.
        if (file.Length > MaxFileSize)
        {
            result.AddError(
                "File",
                "Розмір файлу не повинен перевищувати 10 МБ.");
        }

        var originalFileName = Path.GetFileName(file.FileName);

        // Перевіряє наявність імені файлу.
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            result.AddError(
                "FileName",
                "Ім’я файлу відсутнє.");

            return result;
        }

        // Перевіряє довжину імені файлу.
        if (originalFileName.Length > 255)
        {
            result.AddError(
                "FileName",
                "Ім’я файлу занадто довге.");
        }

        var extension = Path
            .GetExtension(originalFileName)
            .ToLowerInvariant();

        // Перевіряє, чи дозволене розширення файлу.
        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedExtensions.Contains(extension))
        {
            result.AddError(
                "File",
                "Дозволені лише файли PDF, DOCX і TXT.");

            return result;
        }

        // Отримує очікуваний MIME-тип відповідно до розширення.
        if (!AllowedContentTypes.TryGetValue(
                extension,
                out var expectedContentType))
        {
            result.AddError(
                "ContentType",
                "Тип файлу не підтримується.");

            return result;
        }

        // Перевіряє відповідність MIME-типу розширенню файлу.
        if (!string.Equals(
                file.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            result.AddError(
                "ContentType",
                $"MIME-тип файлу не відповідає розширенню {extension}.");
        }

        // Перевіряє сигнатуру фактичного вмісту файлу.
        var hasValidSignature = await HasValidSignatureAsync(
            file,
            extension);

        if (!hasValidSignature)
        {
            result.AddError(
                "File",
                "Вміст файлу не відповідає його розширенню.");
        }

        return result;
    }

    /// <summary>
    /// Перевіряє сигнатуру файлу
    /// відповідно до його розширення.
    /// </summary>
    /// <param name="file">
    /// Файл, сигнатуру якого потрібно перевірити.
    /// </param>
    /// <param name="extension">
    /// Нормалізоване розширення файлу.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо сигнатура відповідає розширенню;
    /// інакше — <see langword="false"/>.
    /// </returns>
    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension)
    {
        await using var stream = file.OpenReadStream();

        return extension switch
        {
            ".pdf" => await StartsWithAsync(
                stream,
                new byte[]
                {
                    0x25, 0x50, 0x44, 0x46
                }),

            ".docx" => await StartsWithAsync(
                stream,
                new byte[]
                {
                    0x50, 0x4B, 0x03, 0x04
                }),

            ".txt" => !ContainsNullByte(stream),

            _ => false
        };
    }

    /// <summary>
    /// Перевіряє, чи починається потік
    /// із заданої послідовності байтів.
    /// </summary>
    /// <param name="stream">
    /// Потік даних файлу.
    /// </param>
    /// <param name="expectedBytes">
    /// Очікувана початкова послідовність байтів.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо початок потоку
    /// відповідає очікуваній сигнатурі;
    /// інакше — <see langword="false"/>.
    /// </returns>
    private static async Task<bool> StartsWithAsync(
        Stream stream,
        byte[] expectedBytes)
    {
        var actualBytes = new byte[expectedBytes.Length];

        var bytesRead = await stream.ReadAsync(
            actualBytes.AsMemory(0, actualBytes.Length));

        return bytesRead == expectedBytes.Length &&
               actualBytes.SequenceEqual(expectedBytes);
    }

    /// <summary>
    /// Перевіряє, чи містить текстовий файл
    /// нульовий байт.
    /// </summary>
    /// <param name="stream">
    /// Потік даних текстового файлу.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо нульовий байт знайдено;
    /// інакше — <see langword="false"/>.
    /// </returns>
    private static bool ContainsNullByte(Stream stream)
    {
        const int bufferSize = 4096;
        var buffer = new byte[bufferSize];

        int bytesRead;

        while ((bytesRead = stream.Read(
                   buffer,
                   0,
                   buffer.Length)) > 0)
        {
            for (var index = 0; index < bytesRead; index++)
            {
                if (buffer[index] == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}