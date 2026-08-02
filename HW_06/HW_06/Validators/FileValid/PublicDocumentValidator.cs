using Microsoft.AspNetCore.Http;

namespace HW_06.Validators.FileValid;

/// <summary>
/// Перевіряє публічні документи
/// перед їх збереженням.
/// </summary>
public static class PublicDocumentValidator
{
    /// <summary>
    /// Максимально допустимий розмір файлу — 10 МБ.
    /// </summary>
    private const long MaxFileSize = 10 * 1024 * 1024;

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
    /// MIME-тип і сигнатуру публічного документа.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно перевірити.
    /// </param>
    /// <returns>
    /// Текст першої знайденої помилки або
    /// <see langword="null"/>, якщо файл пройшов перевірку.
    /// </returns>
    public static async Task<string?> ValidateAsync(IFormFile? file)
    {
        // Перевіряє наявність файлу.
        if (file is null)
        {
            return "Файл не було передано.";
        }

        // Перевіряє, що файл не порожній.
        if (file.Length == 0)
        {
            return "Не можна завантажити порожній файл.";
        }

        // Перевіряє допустимий розмір файлу.
        if (file.Length > MaxFileSize)
        {
            return "Розмір файлу не повинен перевищувати 10 МБ.";
        }

        var originalFileName =
            Path.GetFileName(file.FileName);

        // Перевіряє наявність імені файлу.
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return "Ім’я файлу відсутнє.";
        }

        // Перевіряє довжину імені файлу.
        if (originalFileName.Length > 255)
        {
            return "Ім’я файлу занадто довге.";
        }

        var extension = Path
            .GetExtension(originalFileName)
            .ToLowerInvariant();

        // Перевіряє, чи дозволене розширення файлу.
        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedExtensions.Contains(extension))
        {
            return "Дозволені лише файли PDF, DOCX і TXT.";
        }

        // Отримує очікуваний MIME-тип відповідно до розширення.
        if (!AllowedContentTypes.TryGetValue(
                extension,
                out var expectedContentType))
        {
            return "Тип файлу не підтримується.";
        }

        // Перевіряє відповідність MIME-типу розширенню документа.
        if (!string.Equals(
                file.ContentType,
                expectedContentType,
                StringComparison.OrdinalIgnoreCase))
        {
            return
                $"MIME-тип файлу не відповідає розширенню {extension}.";
        }

        // Перевіряє фактичний вміст документа за його сигнатурою.
        var hasValidSignature =
            await HasValidSignatureAsync(
                file,
                extension);

        if (!hasValidSignature)
        {
            return
                "Вміст файлу не відповідає його розширенню.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє сигнатуру файлу
    /// відповідно до його розширення.
    /// </summary>
    /// <param name="file">
    /// Файл, сигнатуру якого необхідно перевірити.
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
                }), // %PDF

            ".docx" => await StartsWithAsync(
                stream,
                new byte[]
                {
                    0x50, 0x4B, 0x03, 0x04
                }), // ZIP-контейнер

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
        var actualBytes =
            new byte[expectedBytes.Length];

        var bytesRead = await stream.ReadAsync(
            actualBytes.AsMemory(
                0,
                actualBytes.Length));

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