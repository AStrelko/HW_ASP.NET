using AutoMapper;
using HW_06.DTOs.Files;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Queries.GetById;

/// <summary>
/// Обробник запиту для отримання
/// інформації про приватний файл.
/// </summary>
public class GetPrivateAttachmentByIdQueryHandler
    : IRequestHandler<
        GetPrivateAttachmentByIdQuery,
        AttachmentPrivateDTO?>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Ініціалізує обробник запиту.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public GetPrivateAttachmentByIdQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Повертає інформацію про приватний файл,
    /// якщо учасник є його відправником
    /// або отримувачем.
    /// </summary>
    public async Task<AttachmentPrivateDTO?> Handle(
        GetPrivateAttachmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var privateFile =
            await _context.ParticipantPrivateFiles
                .AsNoTracking()
                .Include(file =>
                    file.SenderParticipant)
                .Include(file =>
                    file.RecipientParticipant)
                .FirstOrDefaultAsync(
                    file =>
                        file.Id == request.FileId &&
                        (file.SenderParticipantId ==
                            request.ParticipantId ||
                         file.RecipientParticipantId ==
                            request.ParticipantId),
                    cancellationToken);

        if (privateFile is null)
        {
            return null;
        }

        var dto =
            _mapper.Map<AttachmentPrivateDTO>(
                privateFile);

        return dto with
        {
            DownloadUrl =
                $"/api/participants/" +
                $"{request.ParticipantId}" +
                $"/private-files/{privateFile.Id}/download"
        };
    }
}