using FluentValidation;
using HW_06.Features.Meetings.Common;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.PartialUpdate;

/// <summary>
/// Виконує перевірку даних
/// команди часткового оновлення зустрічі.
/// </summary>
public class PartialUpdateMeetingCommandValidator
    : AbstractValidator<PartialUpdateMeetingCommand>
{
    private const int MeetingDurationHours = 3;

    private readonly DataContext _context;

    /// <summary>
    /// Ініціалізує валідатор команди
    /// часткового оновлення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public PartialUpdateMeetingCommandValidator(
        DataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;

        RuleFor(command =>
                command.Id)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор зустрічі повинен бути більшим за нуль.");

        RuleFor(command =>
                command.Dto.Title)
            .ValidMeetingTitle()
            .When(command =>
                command.Dto.Title is not null);

        RuleFor(command =>
                command.Dto.Description)
            .ValidMeetingDescription()
            .When(command =>
                command.Dto.Description is not null);

        RuleFor(command =>
                command.Dto.DateTime)
            .ValidOptionalMeetingDate()
            .Must(date =>
                !date.HasValue ||
                date.Value.DayOfWeek is not DayOfWeek.Saturday
                    and not DayOfWeek.Sunday)
            .WithMessage(
                "Зустріч не можна запланувати на вихідний день.");

        RuleFor(command =>
                command.Dto.RoomNumber)
            .ValidRoomNumber()
            .When(command =>
                command.Dto.RoomNumber.HasValue);

        RuleFor(command => command)
            .CustomAsync(
                ValidateRoomAvailabilityAsync);
    }

    /// <summary>
    /// Перевіряє доступність кімнати
    /// з урахуванням частково змінених даних.
    /// </summary>
    private async Task ValidateRoomAvailabilityAsync(
        PartialUpdateMeetingCommand command,
        ValidationContext<PartialUpdateMeetingCommand> context,
        CancellationToken cancellationToken)
    {
        var existingMeeting =
            await _context.Meetings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    meeting =>
                        meeting.MeetingId ==
                        command.Id,
                    cancellationToken);

        // Відсутність зустрічі обробить handler як 404.
        if (existingMeeting is null)
        {
            return;
        }

        var effectiveDateTime =
            command.Dto.DateTime ??
            existingMeeting.DateTime;

        var effectiveRoomId =
            existingMeeting.RoomId;

        if (command.Dto.RoomNumber.HasValue)
        {
            var roomNumber =
                command.Dto.RoomNumber.Value;

            var room =
                await _context.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        item =>
                            item.NumberRoom ==
                            roomNumber,
                        cancellationToken);

            if (room is null)
            {
                context.AddFailure(
                    "Dto.RoomNumber",
                    $"Кімнату з номером {roomNumber} не знайдено.");

                return;
            }

            effectiveRoomId =
                room.RoomId;
        }

        // Для онлайн-зустрічі кімнату перевіряти не потрібно.
        if (!effectiveRoomId.HasValue)
        {
            return;
        }

        var requestedEnd =
            effectiveDateTime.AddHours(
                MeetingDurationHours);

        var roomIsOccupied =
            await _context.Meetings
                .AsNoTracking()
                .AnyAsync(
                    meeting =>
                        meeting.MeetingId !=
                        command.Id &&
                        meeting.RoomId ==
                        effectiveRoomId.Value &&
                        meeting.DateTime <
                        requestedEnd &&
                        meeting.DateTime.AddHours(
                            MeetingDurationHours) >
                        effectiveDateTime,
                    cancellationToken);

        if (roomIsOccupied)
        {
            context.AddFailure(
                "Dto.RoomNumber",
                "Обрана кімната вже заброньована "
                + "на зазначений час. "
                + $"Тривалість бронювання зустрічі — "
                + $"{MeetingDurationHours} години.");
        }
    }
}