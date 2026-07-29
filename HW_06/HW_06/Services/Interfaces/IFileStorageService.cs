using HW_06.Models.Files;
using Microsoft.AspNetCore.Http;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Визначає операції локального файлового сховища.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Зберігає новий файл та повертає його серверне ім’я.
    /// </summary>
    Task<string> SaveAsync(
        IFormFile file,
        string folder,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Замінює існуючий файл, зберігаючи основну частину його імені.
    /// </summary>
    /// <returns>
    /// Серверне ім’я файлу без розширення.
    /// </returns>
    Task<string> ReplaceAsync(
        IFormFile file,
        string folder,
        string fileName,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Видаляє файл за його повним серверним ім’ям.
    /// </summary>
    Task DeleteAsync(
        string folder,
        string fileName,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Відкриває файл для читання за його повним серверним ім’ям.
    /// </summary>
    FileDownloadResult? OpenRead(
        string folder,
        string fileName,
        FileAccessLevel accessLevel);
}