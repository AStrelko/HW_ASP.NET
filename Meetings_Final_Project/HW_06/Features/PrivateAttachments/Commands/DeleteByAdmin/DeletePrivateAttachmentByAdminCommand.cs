using MediatR;

namespace HW_06.Features.PrivateAttachments.Commands.DeleteByAdmin;

/// <summary>
/// Команда для видалення приватного файлу
/// адміністратором.
/// </summary>
/// <param name="FileId">
/// Ідентифікатор приватного файлу.
/// </param>
public record DeletePrivateAttachmentByAdminCommand(
    int FileId)
    : IRequest<bool>;