using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Queries.GetByParticipant;

/// <summary>
/// Обробник запиту для отримання
/// зустрічей зазначеного учасника.
/// </summary>
public class GetMeetingsByParticipantQueryHandler
    : IRequestHandler<
        GetMeetingsByParticipantQuery,
        List<MeetingReadDTO>>
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
    public GetMeetingsByParticipantQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує всі зустрічі,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    public async Task<List<MeetingReadDTO>> Handle(
        GetMeetingsByParticipantQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participantExists =
            await _context.Participants
                .AsNoTracking()
                .AnyAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.ParticipantId,
                    cancellationToken);

        if (!participantExists)
        {
            throw new KeyNotFoundException(
                $"Учасника з ідентифікатором " +
                $"{request.ParticipantId} не знайдено.");
        }

        var meetings =
            await _context.Meetings
                .AsNoTracking()
                .Include(meeting =>
                    meeting.Room)
                .Include(meeting =>
                    meeting.MeetingParticipants)
                .Where(meeting =>
                    meeting.MeetingParticipants.Any(
                        meetingParticipant =>
                            meetingParticipant.ParticipantId ==
                            request.ParticipantId))
                .OrderBy(meeting =>
                    meeting.DateTime)
                .ToListAsync(
                    cancellationToken);

        return _mapper.Map<List<MeetingReadDTO>>(
            meetings);
    }
}