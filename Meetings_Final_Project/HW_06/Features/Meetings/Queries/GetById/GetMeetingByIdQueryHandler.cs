using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Queries.GetById;

/// <summary>
/// Обробник запиту для отримання
/// детальної інформації про зустріч.
/// </summary>
public class GetMeetingByIdQueryHandler
    : IRequestHandler<
        GetMeetingByIdQuery,
        MeetingDetailDTO?>
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
    public GetMeetingByIdQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує детальну інформацію
    /// про зустріч за її ідентифікатором.
    /// </summary>
    public async Task<MeetingDetailDTO?> Handle(
        GetMeetingByIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var meeting =
            await _context.Meetings
                .AsNoTracking()
                .Include(meeting =>
                    meeting.Room)
                .Include(meeting =>
                    meeting.MeetingParticipants)
                .ThenInclude(link =>
                    link.Participant)
                .ThenInclude(participant =>
                    participant.ApplicationUser)
                .Include(meeting =>
                    meeting.Attachments)
                .FirstOrDefaultAsync(
                    meeting =>
                        meeting.MeetingId ==
                        request.Id,
                    cancellationToken);

        if (meeting is null)
        {
            return null;
        }

        var dto =
            _mapper.Map<MeetingDetailDTO>(
                meeting);

        var organizer =
            await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ApplicationUserId ==
                        meeting.OrganizerId,
                    cancellationToken);

        if (organizer is not null)
        {
            dto.Organizer =
                new MeetingOrganizerDTO(
                    organizer.ParticipantId,
                    organizer.FirstName,
                    organizer.LastName);
        }

        dto.Attachments =
            dto.Attachments
                .Select(attachment =>
                    attachment with
                    {
                        DownloadUrl =
                            $"/api/meetings/{meeting.MeetingId}" +
                            $"/attachments/{attachment.Id}/download"
                    })
                .ToList();

        return dto;
    }
}