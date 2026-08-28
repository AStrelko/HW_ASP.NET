using HW_06.Common.Constants;

namespace HW_06.Features.Common.Files;

/// <summary>
/// Виконує перевірку файлів аватарів
/// перед їх завантаженням.
/// </summary>
public static class AvatarFileValidator
{
    /// <summary>
    /// Перевіряє коректність файлу аватара
    /// за розміром, розширенням,
    /// MIME-типом та сигнатурою.
    /// </summary>
    /// <param name="file">
    /// Файл аватара, який необхідно перевірити.
    /// </param>
    /// <param name="maxBytes">
    /// Максимально допустимий розмір файлу в байтах.
    /// </param>
    /// <returns>
    /// Повідомлення про помилку або
    /// <see langword="null"/>, якщо файл є коректним.
    /// </returns>
    public static string? ValidateAvatar(
        IFormFile? file,
        long maxBytes)
    {
        var requiredFileError =
            FileValidationExtensions.ValidateRequiredFile(
                file);

        if (requiredFileError is not null)
        {
            return requiredFileError;
        }

        var fileSizeError =
            FileValidationExtensions.ValidateFileSize(
                file!,
                maxBytes);

        if (fileSizeError is not null)
        {
            return fileSizeError;
        }

        var fileNameError =
            FileValidationExtensions.ValidateFileName(
                file!);

        if (fileNameError is not null)
        {
            return fileNameError;
        }

        var extension =
            FileValidationExtensions.GetNormalizedExtension(
                file!);

        if (!FileValidationExtensions.HasAllowedExtension(
                file!,
                FileTypeConstants.AllowedImageExtensions))
        {
            return
                $"Дозволені лише файли: " +
                $"{string.Join(
                    ", ",
                    FileTypeConstants
                        .AllowedImageExtensions)}.";
        }

        if (!FileTypeConstants.AllowedImageMimeTypes.Contains(
                file!.ContentType,
                StringComparer.OrdinalIgnoreCase))
        {
            return
                "Некоректний MIME-тип зображення.";
        }

        if (!ValidateSignature(
                file,
                extension))
        {
            return
                "Вміст файлу не відповідає його розширенню.";
        }

        return null;
    }

    /// <summary>
    /// Перевіряє, чи відповідає сигнатура
    /// зображення заявленому розширенню.
    /// </summary>
    private static bool ValidateSignature(
        IFormFile file,
        string extension)
    {
        if (!FileTypeConstants.ImageSignatures.TryGetValue(
                extension,
                out var signatures))
        {
            return false;
        }

        using var stream =
            file.OpenReadStream();

        var maxSignatureLength =
            signatures.Max(signature =>
                signature.Length);

        var header =
            new byte[maxSignatureLength];

        var bytesRead =
            stream.Read(
                header,
                0,
                header.Length);

        return signatures.Any(signature =>
            bytesRead >= signature.Length &&
            header
                .Take(signature.Length)
                .SequenceEqual(signature));
    }
}