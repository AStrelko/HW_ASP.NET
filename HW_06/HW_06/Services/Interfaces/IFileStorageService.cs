using HW_06.Models.Files;
using Microsoft.AspNetCore.Http;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Визначає операції локального файлового сховища.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Зберігає новий файл у локальному файловому сховищі.
    /// </summary>
    /// <param name="file">
    /// Файл, який необхідно зберегти.
    /// </param>
    /// <param name="folder">
    /// Назва папки, у якій буде збережено файл.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Унікальне серверне ім'я файлу без розширення.
    /// </returns>
    Task<string> SaveAsync(
        IFormFile file,
        string folder,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Замінює існуючий файл, зберігаючи основну частину його імені.
    /// </summary>
    /// <param name="file">
    /// Новий файл, який замінить існуючий.
    /// </param>
    /// <param name="folder">
    /// Назва папки, у якій зберігається файл.
    /// </param>
    /// <param name="fileName">
    /// Серверне ім'я файлу без розширення.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Серверне ім'я файлу без розширення.
    /// </returns>
    Task<string> ReplaceAsync(
        IFormFile file,
        string folder,
        string fileName,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Видаляє файл із локального файлового сховища.
    /// </summary>
    /// <param name="folder">
    /// Назва папки, у якій зберігається файл.
    /// </param>
    /// <param name="fileName">
    /// Серверне ім'я файлу без розширення.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    Task DeleteAsync(
        string folder,
        string fileName,
        FileAccessLevel accessLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Відкриває файл для читання.
    /// </summary>
    /// <param name="folder">
    /// Назва папки, у якій зберігається файл.
    /// </param>
    /// <param name="fileName">
    /// Серверне ім'я файлу без розширення.
    /// </param>
    /// <param name="accessLevel">
    /// Рівень доступу до файлу.
    /// </param>
    /// <returns>
    /// Інформацію про файл та потік для його читання або
    /// <see langword="null"/>, якщо файл не знайдено.
    /// </returns>
    FileDownloadResult? OpenRead(
        string folder,
        string fileName,
        FileAccessLevel accessLevel);
}