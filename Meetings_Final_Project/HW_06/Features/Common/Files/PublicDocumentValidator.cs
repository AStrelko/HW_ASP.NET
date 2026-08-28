using HW_06.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace HW_06.Features.Common.Files;

/// <summary>
/// Перевіряє публічні документи зустрічей
/// перед їх збереженням.
/// </summary>
public static class PublicDocumentValidator
{
    /// <summary>
    /// Перевіряє розмір, ім’я, розширення,
    /// MIME-тип і сигнатуру публічного документа.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно перевірити.
    /// </param>
    /// <param name="maxBytes">
    /// Максимально допустимий розмір файлу в байтах.
    /// </param>
    /// <returns>
    /// Текст першої знайденої помилки або
    /// <see langword="null"/>, якщо файл пройшов перевірку.
    /// </returns>
    public static async Task<string?> ValidateAsync(
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
                FileTypeConstants.AllowedDocumentExtensions))
        {
            return
                $"Дозволені лише файли: " +
                $"{string.Join(
                    ", ",
                    FileTypeConstants.AllowedDocumentExtensions)}.";
        }

        if (!FileValidationExtensions.HasValidContentType(
                file!,
                FileTypeConstants.DocumentMimeTypes))
        {
            return
                $"MIME-тип файлу не відповідає " +
                $"розширенню {extension}.";
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