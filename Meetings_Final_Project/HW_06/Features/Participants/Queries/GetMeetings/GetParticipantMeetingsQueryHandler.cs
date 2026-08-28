using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Queries.GetMeetings;

/// <summary>
/// Обробник запиту для отримання
/// зустрічей зазначеного учасника.
/// </summary>
public class GetParticipantMeetingsQueryHandler
    : IRequestHandler<
        GetParticipantMeetingsQuery,
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
    public GetParticipantMeetingsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує список зустрічей,
    /// у яких бере участь зазначений учасник.
    /// </summary>
    public async Task<List<MeetingReadDTO>> Handle(
        GetParticipantMeetingsQuery request,
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

        var result =
            _mapper.Map<List<MeetingReadDTO>>(
                meetings);

        var participantCounts =
            meetings.ToDictionary(
                meeting =>
                    meeting.MeetingId,
                meeting =>
                    meeting.MeetingParticipants.Count);

        foreach (var meeting in result)
        {
            meeting.ParticipantsCount =
                participantCounts.GetValueOrDefault(
                    meeting.MeetingId);
        }

        return result;
    }
}