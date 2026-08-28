using AutoMapper;
using HW_06.DTOs.ParticipantDTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Queries.GetById;

/// <summary>
/// Обробник запиту для отримання
/// детальної інформації про учасника.
/// </summary>
public class GetParticipantByIdQueryHandler
    : IRequestHandler<
        GetParticipantByIdQuery,
        ParticipantDetailDTO?>
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
    public GetParticipantByIdQueryHandler(
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
    /// про учасника.
    /// </summary>
    public async Task<ParticipantDetailDTO?> Handle(
        GetParticipantByIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participant =
            await _context.Participants
                .AsNoTracking()
                .Include(participant =>
                    participant.ApplicationUser)
                .Include(participant =>
                    participant.MeetingParticipants)
                .ThenInclude(meetingParticipant =>
                    meetingParticipant.Meeting)
                .ThenInclude(meeting =>
                    meeting.Room)
                .Include(participant =>
                    participant.SentPrivateFiles)
                .ThenInclude(file =>
                    file.RecipientParticipant)
                .Include(participant =>
                    participant.ReceivedPrivateFiles)
                .ThenInclude(file =>
                    file.SenderParticipant)
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.Id,
                    cancellationToken);

        if (participant is null)
        {
            return null;
        }

        foreach (var file
                 in participant.SentPrivateFiles)
        {
            file.SenderParticipant =
                participant;
        }

        foreach (var file
                 in participant.ReceivedPrivateFiles)
        {
            file.RecipientParticipant =
                participant;
        }

        var result =
            _mapper.Map<ParticipantDetailDTO>(
                participant);

        var meetingIds =
            participant.MeetingParticipants
                .Select(meetingParticipant =>
                    meetingParticipant.MeetingId)
                .ToList();

        if (meetingIds.Count > 0)
        {
            var participantCounts =
                await _context.MeetingParticipants
                    .AsNoTracking()
                    .Where(meetingParticipant =>
                        meetingIds.Contains(
                            meetingParticipant.MeetingId))
                    .GroupBy(meetingParticipant =>
                        meetingParticipant.MeetingId)
                    .Select(group =>
                        new
                        {
                            MeetingId =
                                group.Key,

                            ParticipantsCount =
                                group.Count()
                        })
                    .ToDictionaryAsync(
                        item =>
                            item.MeetingId,

                        item =>
                            item.ParticipantsCount,

                        cancellationToken);

            foreach (var meeting
                     in result.Meetings)
            {
                meeting.ParticipantsCount =
                    participantCounts
                        .GetValueOrDefault(
                            meeting.MeetingId);
            }
        }

        result.SentPrivateFiles =
            result.SentPrivateFiles
                .Select(file =>
                    file with
                    {
                        DownloadUrl =
                            $"/api/participants/{participant.ParticipantId}" +
                            $"/private-files/{file.Id}/download"
                    })
                .ToList();

        result.ReceivedPrivateFiles =
            result.ReceivedPrivateFiles
                .Select(file =>
                    file with
                    {
                        DownloadUrl =
                            $"/api/participants/{participant.ParticipantId}" +
                            $"/private-files/{file.Id}/download"
                    })
                .ToList();

        return result;
    }
}