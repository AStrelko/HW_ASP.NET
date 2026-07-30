using HW_06.Models.Files;
using HW_06.Services.Interfaces;

namespace HW_06.Services;

/// <summary>
/// Сервіс локального зберігання публічних і приватних файлів.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    /// <summary>
    /// Розширення файлів, які підтримує файлове сховище.
    /// </summary>
    private static readonly string[] SupportedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    private readonly string _publicRoot;
    private readonly string _privateRoot;

    /// <summary>
    /// Ініціалізує локальне файлове сховище
    /// та створює каталоги для публічних і приватних файлів.
    /// </summary>
    /// <param name="environment">
    /// Інформація про середовище виконання вебзастосунку.
    /// </param>
    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        _publicRoot = Path.Combine(
            environment.ContentRootPath,
            "uploads",
            "PublicFiles");

        _privateRoot = Path.Combine(
            environment.ContentRootPath,
            "uploads",
            "PrivateFiles");

        Directory.CreateDirectory(_publicRoot);
        Directory.CreateDirectory(_privateRoot);
    }

    /// <summary>
    /// Зберігає новий файл у локальному файловому сховищі.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно зберегти.
    /// </param>
    /// <param name="folder">
    /// Назва каталогу для зберігання файлу.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Унікальне серверне ім’я збереженого файлу без розширення.
    /// </returns>
    public async Task<string> SaveAsync(
        IFormFile file,
        string folder,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        cancellationToken.ThrowIfCancellationRequested();

        var extension = GetNormalizedExtension(file.FileName);
        var fileId = Guid.NewGuid().ToString("N");

        var directory = ResolveDirectory(
            folder,
            accessLevel);

        Directory.CreateDirectory(directory);

        var fullFileName = $"{fileId}{extension}";

        var fullPath = ResolveSafePath(
            folder,
            fullFileName,
            accessLevel);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        return fileId;
    }

    /// <summary>
    /// Видаляє файл за його унікальним ідентифікатором.
    /// Розширення файлу визначається автоматично.
    /// </summary>
    /// <param name="folder">
    /// Назва каталогу, у якому зберігається файл.
    /// </param>
    /// <param name="fileId">
    /// Унікальний ідентифікатор файлу без розширення.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    public Task DeleteAsync(
        string folder,
        string fileId,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ValidateFileId(fileId);

        var existingFilePath = FindExistingFilePath(
            folder,
            fileId,
            accessLevel);

        if (existingFilePath is not null)
        {
            File.Delete(existingFilePath);
        }

        return Task.CompletedTask;
    }

   /// <summary>
/// Замінює існуючий файл, знаходячи його
/// за серверним ім’ям без розширення.
/// </summary>
/// <param name="file">
/// Новий файл.
/// </param>
/// <param name="folder">
/// Каталог, у якому зберігається файл.
/// </param>
/// <param name="baseFileName">
/// Серверне ім’я існуючого файлу без розширення.
/// </param>
/// <param name="accessLevel">
/// Рівень доступу до файлу.
/// </param>
/// <param name="cancellationToken">
/// Токен скасування операції.
/// </param>
/// <returns>
/// Актуальне серверне ім’я файлу разом із розширенням.
/// </returns>
public async Task<string> ReplaceAsync(
    IFormFile file,
    string folder,
    string baseFileName,
    FileAccessLevel accessLevel,
    CancellationToken cancellationToken = default)
{
    ValidateFile(file);
    ValidateBaseFileName(baseFileName);

    cancellationToken.ThrowIfCancellationRequested();

    var existingFilePath = FindExistingFilePath(
        folder,
        baseFileName,
        accessLevel);

    if (existingFilePath is null)
    {
        throw new FileNotFoundException(
            "Файл для заміни не знайдено.",
            baseFileName);
    }

    var newExtension = GetNormalizedExtension(
        file.FileName);

    var newFileName =
        $"{baseFileName}{newExtension}";

    var newFilePath = ResolveSafePath(
        folder,
        newFileName,
        accessLevel);

    var extensionChanged = !string.Equals(
        existingFilePath,
        newFilePath,
        StringComparison.OrdinalIgnoreCase);

    if (!extensionChanged)
    {
        await using var replacementStream = new FileStream(
            existingFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await file.CopyToAsync(
            replacementStream,
            cancellationToken);

        return baseFileName;
    }

    if (File.Exists(newFilePath))
    {
        throw new IOException(
            $"Файл з ім’ям '{newFileName}' уже існує.");
    }

    await using (var newFileStream = new FileStream(
                     newFilePath,
                     FileMode.CreateNew,
                     FileAccess.Write,
                     FileShare.None))
    {
        await file.CopyToAsync(
            newFileStream,
            cancellationToken);
    }

    File.Delete(existingFilePath);

    return baseFileName;
}

        /// <summary>
        /// Відкриває файл для читання.
        /// </summary>
        /// <param name="folder">
        /// Назва каталогу, у якому зберігається файл.
        /// </param>
        /// <param name="fileName">
        /// Серверне ім’я файлу без розширення.
        /// </param>
        /// <param name="accessLevel">
        /// Рівень доступу до файлу.
        /// </param>
        /// <returns>
        /// Об’єкт із потоком даних, MIME-типом та ім’ям файлу
        /// або <see langword="null"/>, якщо файл не знайдено.
        /// </returns>
        public FileDownloadResult? OpenRead(
        string folder,
        string baseFileName,
        FileAccessLevel accessLevel)
    {
        ValidateBaseFileName(baseFileName);

        var filePath = FindExistingFilePath(
            folder,
            baseFileName,
            accessLevel);

        if (filePath is null)
        {
            return null;
        }

        var extension = Path
            .GetExtension(filePath)
            .ToLowerInvariant();

        return new FileDownloadResult
        {
            Content = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read),

            ContentType = GetContentType(extension),

            FileName = Path.GetFileName(filePath)
        };
    }

    /// <summary>
    /// Перевіряє, чи передано коректний непорожній файл.
    /// </summary>
    /// <param name="file">
    /// Файл для перевірки.
    /// </param>
    private static void ValidateFile(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length == 0)
        {
            throw new ArgumentException(
                "Файл не повинен бути порожнім.",
                nameof(file));
        }
    }

    /// <summary>
    /// Перевіряє унікальний ідентифікатор файлу.
    /// Ідентифікатор не повинен містити розширення або частини шляху.
    /// </summary>
    /// <param name="fileId">
    /// Унікальний ідентифікатор файлу.
    /// </param>
    private static void ValidateFileId(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            throw new ArgumentException(
                "Ідентифікатор файлу не вказано.",
                nameof(fileId));
        }

        if (!string.Equals(
                fileId,
                Path.GetFileName(fileId),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Ідентифікатор файлу містить неприпустимі символи шляху.",
                nameof(fileId));
        }

        if (!string.IsNullOrEmpty(
                Path.GetExtension(fileId)))
        {
            throw new ArgumentException(
                "Ідентифікатор файлу необхідно передавати без розширення.",
                nameof(fileId));
        }
    }

    /// <summary>
    /// Отримує та перевіряє розширення завантаженого файлу.
    /// </summary>
    /// <param name="fileName">
    /// Початкове ім'я завантаженого файлу.
    /// </param>
    /// <returns>
    /// Нормалізоване розширення файлу в нижньому регістрі.
    /// </returns>
    private static string GetNormalizedExtension(
        string fileName)
    {
        var extension = Path
            .GetExtension(fileName)
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException(
                "Файл повинен мати розширення.",
                nameof(fileName));
        }

        if (!SupportedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Розширення '{extension}' не підтримується. " +
                $"Дозволені формати: {string.Join(", ", SupportedExtensions)}.",
                nameof(fileName));
        }

        return extension;
    }

    /// <summary>
    /// Шукає файл за його ім’ям без розширення
    /// серед усіх підтримуваних форматів.
    /// </summary>
    /// <param name="folder">
    /// Каталог, у якому виконується пошук.
    /// </param>
    /// <param name="baseFileName">
    /// Серверне ім’я файлу без розширення.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <returns>
    /// Повний шлях до знайденого файлу або
    /// <see langword="null"/>, якщо файл не знайдено.
    /// </returns>
    private string? FindExistingFilePath(
        string folder,
        string baseFileName,
        FileAccessLevel accessLevel)
    {
        ValidateBaseFileName(baseFileName);

        foreach (var extension in SupportedExtensions)
        {
            var fullFileName = $"{baseFileName}{extension}";

            var fullPath = ResolveSafePath(
                folder,
                fullFileName,
                accessLevel);

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    /// <summary>
    /// Визначає MIME-тип файлу за його розширенням.
    /// </summary>
    /// <param name="extension">
    /// Розширення файлу.
    /// </param>
    /// <returns>
    /// MIME-тип файлу.
    /// </returns>
    private static string GetContentType(
        string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",

            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Визначає безпечний каталог зберігання
    /// відповідно до рівня доступу.
    /// </summary>
    /// <param name="folder">
    /// Назва вкладеного каталогу.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлів.
    /// </param>
    /// <returns>
    /// Абсолютний шлях до каталогу.
    /// </returns>
    private string ResolveDirectory(
        string folder,
        FileAccessLevel accessLevel)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new ArgumentException(
                "Назву каталогу не вказано.",
                nameof(folder));
        }

        var root = RootFor(accessLevel);

        var normalizedRoot = Path.GetFullPath(root);

        var directory = Path.GetFullPath(
            Path.Combine(normalizedRoot, folder));

        if (!directory.StartsWith(
                normalizedRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Спроба виходу за межі каталогу зберігання.");
        }

        return directory;
    }

    /// <summary>
    /// Формує безпечний абсолютний шлях до файлу
    /// та запобігає виходу за межі каталогу зберігання.
    /// </summary>
    /// <param name="folder">
    /// Назва вкладеного каталогу.
    /// </param>
    /// <param name="fileName">
    /// Повне ім'я файлу разом із розширенням.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <returns>
    /// Безпечний абсолютний шлях до файлу.
    /// </returns>
    private string ResolveSafePath(
        string folder,
        string fileName,
        FileAccessLevel accessLevel)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Ім'я файлу не вказано.",
                nameof(fileName));
        }

        var directory = ResolveDirectory(
            folder,
            accessLevel);

        var normalizedDirectory =
            Path.GetFullPath(directory);

        var fullPath = Path.GetFullPath(
            Path.Combine(
                normalizedDirectory,
                fileName));

        if (!fullPath.StartsWith(
                normalizedDirectory + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Спроба виходу за межі каталогу зберігання.");
        }

        return fullPath;
    }

    /// <summary>
    /// Повертає кореневий каталог відповідно
    /// до рівня доступу до файлу.
    /// </summary>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <returns>
    /// Кореневий каталог публічного або приватного сховища.
    /// </returns>
    private string RootFor(
        FileAccessLevel accessLevel)
    {
        return accessLevel switch
        {
            FileAccessLevel.Public => _publicRoot,
            FileAccessLevel.Private => _privateRoot,

            _ => throw new ArgumentOutOfRangeException(
                nameof(accessLevel),
                accessLevel,
                "Невідомий рівень доступу до файлу.")
        };
    }
    
    /// <summary>
    /// Перевіряє серверне ім’я файлу без розширення.
    /// </summary>
    /// <param name="baseFileName">
    /// Ім’я файлу без розширення.
    /// </param>
    private static void ValidateBaseFileName(
        string baseFileName)
    {
        if (string.IsNullOrWhiteSpace(baseFileName))
        {
            throw new ArgumentException(
                "Ім’я файлу не вказано.",
                nameof(baseFileName));
        }

        if (!string.Equals(
                baseFileName,
                Path.GetFileName(baseFileName),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Ім’я файлу містить неприпустимі символи шляху.",
                nameof(baseFileName));
        }

        if (!string.IsNullOrWhiteSpace(
                Path.GetExtension(baseFileName)))
        {
            throw new ArgumentException(
                "Ім’я файлу необхідно передавати без розширення.",
                nameof(baseFileName));
        }
    }
}