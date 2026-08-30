using FluentValidation;
using HW_06.Features.Meetings.Common;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.Create;

/// <summary>
/// Виконує перевірку даних
/// команди створення зустрічі.
/// </summary>
public class CreateMeetingCommandValidator
    : AbstractValidator<CreateMeetingCommand>
{
    private const int MeetingDurationHours = 3;

    private readonly DataContext _context;

    /// <summary>
    /// Ініціалізує валідатор команди
    /// створення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    public CreateMeetingCommandValidator(
        DataContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;

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
    /// зазначеної кімнати.
    /// </summary>
    private async Task ValidateRoomAvailabilityAsync(
        CreateMeetingCommand command,
        ValidationContext<CreateMeetingCommand> context,
        CancellationToken cancellationToken)
    {
        var roomNumber =
            command.Dto.RoomNumber;

        // Кімната не вказана — зустріч вважається онлайн.
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
                + $"вже заброньована на зазначений час. "
                + $"Тривалість бронювання зустрічі — "
                + $"{MeetingDurationHours} години.");
        }
    }
}