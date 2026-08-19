using Microsoft.AspNetCore.Http;

namespace HW_06.Validators.FileValid;

/// <summary>
/// Перевіряє приватні документи учасників
/// перед їх збереженням.
/// </summary>
public static class PrivateDocumentValidator
{
    /// <summary>
    /// Максимально допустимий розмір файлу — 10 МБ.
    /// </summary>
    private const long MaxFileSize =
        10 * 1024 * 1024;

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
            [".pdf"] =
                "application/pdf",

            [".docx"] =
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            [".txt"] =
                "text/plain"
        };

    /// <summary>
    /// Перевіряє розмір, ім’я, розширення,
    /// MIME-тип і сигнатуру приватного документа.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно перевірити.
    /// </param>
    /// <returns>
    /// Текст першої знайденої помилки або
    /// <see langword="null"/>, якщо файл пройшов перевірку.
    /// </returns>
    public static async Task<string?> ValidateAsync(
        IFormFile? file)
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
                MaxFileSize);

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
                AllowedExtensions))
        {
            return
                "Дозволені лише файли PDF, DOCX і TXT.";
        }

        if (!FileValidationExtensions.HasValidContentType(
                file!,
                AllowedContentTypes))
        {
            return
                $"MIME-тип файлу не відповідає розширенню {extension}.";
        }

        var hasValidSignature =
            await FileValidationExtensions
                .HasValidDocumentSignatureAsync(
                    file!,
                    extension);

        if (!hasValidSignature)
        {
            return
                "Вміст файлу не відповідає його розширенню.";
        }

        return null;
    }
}