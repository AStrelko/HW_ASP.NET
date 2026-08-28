using AutoMapper;
using HW_06.DTOs.Files;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.PrivateAttachments.Queries.GetAll;

/// <summary>
/// Обробник запиту для отримання
/// списку всіх приватних файлів.
/// </summary>
public class GetAllPrivateAttachmentsQueryHandler
    : IRequestHandler<
        GetAllPrivateAttachmentsQuery,
        IReadOnlyCollection<AttachmentPrivateDTO>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Ініціалізує обробник запиту
    /// на отримання всіх приватних файлів.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public GetAllPrivateAttachmentsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Повертає список
    /// усіх приватних файлів.
    /// </summary>
    public async Task<
        IReadOnlyCollection<AttachmentPrivateDTO>> Handle(
        GetAllPrivateAttachmentsQuery request,
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
                        $"{file.RecipientParticipantId}" +
                        $"/private-files/{file.Id}/download"
                };
            })
            .ToList();
    }
}