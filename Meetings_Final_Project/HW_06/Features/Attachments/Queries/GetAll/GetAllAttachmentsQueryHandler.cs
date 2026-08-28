using AutoMapper;
using HW_06.DTOs.Files;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Attachments.Queries.GetAll;

/// <summary>
/// Обробник запиту для отримання
/// публічних файлів зустрічі.
/// </summary>
public class GetAllAttachmentsQueryHandler
    : IRequestHandler<
        GetAllAttachmentsQuery,
        IReadOnlyCollection<AttachmentPublicDTO>?>
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
    public GetAllAttachmentsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Повертає всі публічні файли,
    /// прикріплені до зазначеної зустрічі.
    /// </summary>
    public async Task<
        IReadOnlyCollection<AttachmentPublicDTO>?> Handle(
        GetAllAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var meetingExists =
            await _context.Meetings
                .AsNoTracking()
                .AnyAsync(
                    meeting =>
                        meeting.MeetingId ==
                        request.MeetingId,
                    cancellationToken);

        if (!meetingExists)
        {
            return null;
        }

        var attachments =
            await _context.MeetingAttachments
                .AsNoTracking()
                .Where(attachment =>
                    attachment.MeetingId ==
                    request.MeetingId)
                .OrderByDescending(attachment =>
                    attachment.UploadedAtUtc)
                .ToListAsync(
                    cancellationToken);

        return attachments
            .Select(attachment =>
            {
                var dto =
                    _mapper.Map<AttachmentPublicDTO>(
                        attachment);

                return dto with
                {
                    DownloadUrl =
                        $"/api/meetings/{request.MeetingId}" +
                        $"/attachments/{attachment.Id}/download"
                };
            })
            .ToList();
    }
}