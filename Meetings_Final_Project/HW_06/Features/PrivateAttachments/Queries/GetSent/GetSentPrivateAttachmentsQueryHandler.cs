using AutoMapper;
using HW_06.DTOs.Files;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Queries.GetSent;

/// <summary>
/// Обробник запиту для отримання
/// приватних файлів, надісланих учасником.
/// </summary>
public class GetSentPrivateAttachmentsQueryHandler
    : IRequestHandler<
        GetSentPrivateAttachmentsQuery,
        IReadOnlyCollection<AttachmentPrivateDTO>>
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
    public GetSentPrivateAttachmentsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Повертає приватні файли,
    /// надіслані зазначеним учасником.
    /// </summary>
    public async Task<
        IReadOnlyCollection<AttachmentPrivateDTO>> Handle(
        GetSentPrivateAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var privateFiles =
            await _context.ParticipantPrivateFiles
                .AsNoTracking()
                .Include(file =>
                    file.SenderParticipant)
                .Include(file =>
                    file.RecipientParticipant)
                .Where(file =>
                    file.SenderParticipantId ==
                    request.ParticipantId)
                .OrderByDescending(file =>
                    file.UploadedAtUtc)
                .ToListAsync(
                    cancellationToken);

        return privateFiles
            .Select(file =>
            {
                var dto =
                    _mapper.Map<AttachmentPrivateDTO>(
                        file);

                return dto with
                {
                    DownloadUrl =
                        $"/api/participants/" +
                        $"{request.ParticipantId}" +
                        $"/private-files/{file.Id}/download"
                };
            })
            .ToList();
    }
}