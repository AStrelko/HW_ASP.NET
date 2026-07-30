namespace HW_06.Validators.FileValid;

/// <summary>
/// Виконує перевірку файлів аватарів перед їх завантаженням.
/// </summary>
public static class AvatarFileValidator
{
    private static readonly string[] allowedExtensions = [".jpg", ".png", ".jpeg", ".webp"];
    private static readonly string[] ImageMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    private static readonly Dictionary<string, byte[][]> Signatures = new()
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]]
    };

    /// <summary>
    /// Перевіряє коректність файлу аватара за розміром,
    /// розширенням, MIME-типом та сигнатурою.
    /// </summary>
    /// <param name="file">
    /// Файл аватара, який необхідно перевірити.
    /// </param>
    /// <param name="maxBytes">
    /// Максимально допустимий розмір файлу в байтах.
    /// </param>
    /// <returns>
    /// Повідомлення про помилку, якщо перевірку не пройдено,
    /// або <see langword="null"/>, якщо файл є коректним.
    /// </returns>
    public static string? ValidateAvatar(IFormFile file, long maxBytes)
    {
        if (file is null || file.Length == 0) return "Файл не вибран.";
        if (file.Length > maxBytes) return $"Розмір файла більше за {maxBytes / 1024 / 1024} МБ.";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension)) return "Некоректне розширення.";

        if (!ImageMimeTypes.Contains(file.ContentType)) return "Некоректний тип файлу.";

        if (!ValidateSignature(file, extension)) return "Вміст файлу не відповідає його розширенню.";

        return null;

    }

    /// <summary>
    /// Перевіряє, чи відповідає сигнатура файлу
    /// заявленому розширенню.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно перевірити.
    /// </param>
    /// <param name="ext">
    /// Розширення файлу.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо сигнатура відповідає
    /// розширенню; інакше — <see langword="false"/>.
    /// </returns>
    private static bool ValidateSignature(IFormFile file, string ext)
    {
        if (!Signatures.TryGetValue(ext, out var signature)) return false;
        
        using var stream = file.OpenReadStream();
        var header = new byte[8];
        var read = stream.Read(header, 0, header.Length);

        return signature.Any(sig => read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));
    }
}