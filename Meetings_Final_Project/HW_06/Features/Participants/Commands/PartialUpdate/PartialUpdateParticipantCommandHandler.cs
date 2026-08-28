using HW_06.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Commands.PartialUpdate;

/// <summary>
/// Обробник команди часткового
/// оновлення учасника.
/// </summary>
public class PartialUpdateParticipantCommandHandler
    : IRequestHandler<
        PartialUpdateParticipantCommand,
        bool>
{
    private readonly DataContext _context;

    private readonly ILogger<PartialUpdateParticipantCommandHandler>
        _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// часткового оновлення учасника.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання подій
    /// часткового оновлення учасника.
    /// </param>
    public PartialUpdateParticipantCommandHandler(
        DataContext context,
        ILogger<PartialUpdateParticipantCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Частково оновлює дані учасника.
    /// Змінюються лише передані поля.
    /// </summary>
    public async Task<bool> Handle(
        PartialUpdateParticipantCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var participant =
            await _context.Participants
                .Include(participant =>
                    participant.MeetingParticipants)
                .FirstOrDefaultAsync(
                    participant =>
                        participant.ParticipantId ==
                        request.Id,
                    cancellationToken);

        if (participant is null)
        {
            _logger.LogWarning(
                "Не вдалося частково оновити учасника. ParticipantId: {ParticipantId} не знайдено.",
                request.Id);

            return false;
        }

        if (dto.FirstName is not null)
        {
            participant.FirstName =
                dto.FirstName.Trim();
        }

        if (dto.LastName is not null)
        {
            participant.LastName =
                dto.LastName.Trim();
        }

        if (dto.Position is not null)
        {
            participant.Position =
                dto.Position.Trim();
        }

        if (dto.MeetingIds is not null)
        {
            var meetingIds =
                dto.MeetingIds
                    .Distinct()
                    .ToList();

            await ValidateMeetingIdsAsync(
                meetingIds,
                cancellationToken);

            participant.MeetingParticipants.Clear();

            foreach (var meetingId in meetingIds)
            {
                participant.MeetingParticipants.Add(
                    new MeetingParticipant
                    {
                        ParticipantId =
                            participant.ParticipantId,

                        MeetingId =
                            meetingId
                    });
            }
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Учасника успішно частково оновлено. ParticipantId: {ParticipantId}, Name: {FirstName} {LastName}",
            participant.ParticipantId,
            participant.FirstName,
            participant.LastName);

        return true;
    }

    /// <summary>
    /// Перевіряє, що всі передані
    /// ідентифікатори зустрічей існують.
    /// </summary>
    private async Task ValidateMeetingIdsAsync(
        List<int> meetingIds,
        CancellationToken cancellationToken)
    {
        if (meetingIds.Count == 0)
        {
            return;
        }

        var existingMeetingIds =
            await _context.Meetings
                .AsNoTracking()
                .Where(meeting =>
                    meetingIds.Contains(
                        meeting.MeetingId))
                .Select(meeting =>
                    meeting.MeetingId)
                .ToListAsync(
                    cancellationToken);

        var missingMeetingIds =
            meetingIds
                .Except(existingMeetingIds)
                .ToList();

        if (missingMeetingIds.Count > 0)
        {
            throw new FluentValidation.ValidationException(
                $"Зустрічі не знайдено: " +
                $"{string.Join(", ", missingMeetingIds)}.");
        }
    }
}