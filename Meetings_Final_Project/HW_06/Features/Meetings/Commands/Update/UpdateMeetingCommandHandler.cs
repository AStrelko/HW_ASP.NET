using FluentValidation;
using FluentValidation.Results;
using HW_06.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.Update;

/// <summary>
/// Обробник команди повного оновлення зустрічі.
/// </summary>
public class UpdateMeetingCommandHandler
    : IRequestHandler<UpdateMeetingCommand, bool>
{
    private readonly DataContext _context;
    private readonly ILogger<UpdateMeetingCommandHandler> _logger;

    /// <summary>
    /// Ініціалізує обробник команди
    /// повного оновлення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
   public UpdateMeetingCommandHandler(
       DataContext context,
       ILogger<UpdateMeetingCommandHandler> logger)
   {
       ArgumentNullException.ThrowIfNull(context);
       ArgumentNullException.ThrowIfNull(logger);
   
       _context = context;
       _logger = logger;
   }

    /// <summary>
    /// Повністю оновлює існуючу зустріч.
    /// Перевіряє існування кімнати
    /// та всіх зазначених учасників.
    /// </summary>
    public async Task<bool> Handle(
        UpdateMeetingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var meeting =
            await _context.Meetings
                .Include(item =>
                    item.MeetingParticipants)
                .FirstOrDefaultAsync(
                    item =>
                        item.MeetingId ==
                        request.Id,
                    cancellationToken);

        if (meeting is null)
        {
            _logger.LogWarning(
                "Не вдалося оновити зустріч. MeetingId: {MeetingId} не знайдено.",
                request.Id);

            return false;
        }

        var validationFailures =
            new List<ValidationFailure>();

        //
        // Перевірка кімнати
        //

        Room? room = null;

        if (dto.RoomNumber.HasValue)
        {
            room =
                await _context.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        item =>
                            item.NumberRoom ==
                            dto.RoomNumber.Value,
                        cancellationToken);

            if (room is null)
            {
                validationFailures.Add(
                    new ValidationFailure(
                        "Dto.RoomNumber",
                        $"Кімнату з номером " +
                        $"{dto.RoomNumber.Value} не знайдено."));
            }
        }

        //
        // Перевірка учасників
        //

        var participantIds =
            dto.ParticipantIds
                .Distinct()
                .ToList();

        var existingParticipantIds =
            await _context.Participants
                .AsNoTracking()
                .Where(participant =>
                    participantIds.Contains(
                        participant.ParticipantId))
                .Select(participant =>
                    participant.ParticipantId)
                .ToListAsync(
                    cancellationToken);

        var missingParticipantIds =
            participantIds
                .Except(existingParticipantIds)
                .ToList();

        if (missingParticipantIds.Count > 0)
        {
            validationFailures.Add(
                new ValidationFailure(
                    "Dto.ParticipantIds",
                    $"Не знайдено учасників " +
                    $"з ідентифікаторами: " +
                    $"{string.Join(", ", missingParticipantIds)}."));
        }

        //
        // Якщо є помилки —
        // повертаємо їх одним ValidationException
        //

        if (validationFailures.Count > 0)
        {
            throw new ValidationException(
                validationFailures);
        }

        //
        // Оновлення зустрічі
        //

        meeting.Title =
            dto.Title;

        meeting.Description =
            dto.Description;

        meeting.DateTime =
            dto.DateTime;

        meeting.RoomId =
            room?.RoomId;

        //
        // Повністю замінюємо список учасників
        //

        meeting.MeetingParticipants.Clear();

        foreach (var participantId
                 in participantIds)
        {
            meeting.MeetingParticipants.Add(
                new MeetingParticipant
                {
                    MeetingId =
                        meeting.MeetingId,

                    ParticipantId =
                        participantId
                });
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Зустріч успішно оновлено. MeetingId: {MeetingId}, Title: {Title}",
            meeting.MeetingId,
            meeting.Title);

        return true;
    }
}