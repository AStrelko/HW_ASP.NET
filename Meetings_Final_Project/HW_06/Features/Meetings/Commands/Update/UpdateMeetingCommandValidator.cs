using FluentValidation;
using HW_06.Features.Meetings.Common;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.Update;

/// <summary>
/// Виконує перевірку команди
/// повного оновлення зустрічі.
/// </summary>
public class UpdateMeetingCommandValidator
    : AbstractValidator<UpdateMeetingCommand>
{
    private const int MeetingDurationHours = 3;

    private readonly DataContext _context;

    /// <summary>
    /// Ініціалізує валідатор команди
    /// повного оновлення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public UpdateMeetingCommandValidator(
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
            .NotEmpty()
            .WithMessage(
                "Назва зустрічі є обов’язковою.")
            .ValidMeetingTitle();

        RuleFor(command =>
                command.Dto.Description)
            .ValidMeetingDescription();

        RuleFor(command =>
                command.Dto.DateTime)
            .ValidMeetingDate()
            .Must(date =>
                date.DayOfWeek is not DayOfWeek.Saturday
                    and not DayOfWeek.Sunday)
            .WithMessage(
                "Зустріч не можна запланувати на вихідний день.");

        RuleFor(command =>
                command.Dto.RoomNumber)
            .ValidRoomNumber();

        RuleFor(command =>
                command.Dto.ParticipantIds)
            .NotNull()
            .WithMessage(
                "Список учасників є обов’язковим.")
            .NotEmpty()
            .WithMessage(
                "Зустріч повинна містити хоча б одного учасника.")
            .ValidParticipantIds();

        RuleFor(command => command)
            .CustomAsync(
                ValidateRoomAvailabilityAsync);
    }

    /// <summary>
    /// Перевіряє існування та доступність
    /// кімнати під час повного оновлення зустрічі.
    /// </summary>
    private async Task ValidateRoomAvailabilityAsync(
        UpdateMeetingCommand command,
        ValidationContext<UpdateMeetingCommand> context,
        CancellationToken cancellationToken)
    {
        var roomNumber =
            command.Dto.RoomNumber;

        // Якщо кімнату не вказано,
        // зустріч вважається онлайн.
        if (!roomNumber.HasValue)
        {
            return;
        }

        var room =
            await _context.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    item =>
                        item.NumberRoom ==
                        roomNumber.Value,
                    cancellationToken);

        if (room is null)
        {
            context.AddFailure(
                "Dto.RoomNumber",
                $"Кімнату з номером {roomNumber.Value} не знайдено.");

            return;
        }

        var requestedStart =
            command.Dto.DateTime;

        var requestedEnd =
            requestedStart.AddHours(
                MeetingDurationHours);

        var roomIsOccupied =
            await _context.Meetings
                .AsNoTracking()
                .AnyAsync(
                    meeting =>
                        meeting.MeetingId !=
                        command.Id &&
                        meeting.RoomId ==
                        room.RoomId &&
                        meeting.DateTime <
                        requestedEnd &&
                        meeting.DateTime.AddHours(
                            MeetingDurationHours) >
                        requestedStart,
                    cancellationToken);

        if (roomIsOccupied)
        {
            context.AddFailure(
                "Dto.RoomNumber",
                $"Кімната з номером {roomNumber.Value} "
                + "вже заброньована на зазначений час. "
                + $"Тривалість бронювання зустрічі — "
                + $"{MeetingDurationHours} години.");
        }
    }
}