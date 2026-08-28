using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Meetings.Commands.Create;

/// <summary>
/// Обробник команди створення зустрічі.
/// </summary>
public class CreateMeetingCommandHandler
    : IRequestHandler<
        CreateMeetingCommand,
        MeetingReadDTO>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateMeetingCommandHandler> _logger;

    /// <summary>
    /// Ініціалізує обробник команди створення зустрічі.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public CreateMeetingCommandHandler(
        DataContext context,
        IMapper mapper,
        ILogger<CreateMeetingCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Створює нову зустріч
    /// та додає до неї зазначених учасників.
    /// </summary>
    public async Task<MeetingReadDTO> Handle(
        CreateMeetingCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dto =
            request.Dto;

        var room =
            await GetRoomAsync(
                dto.RoomNumber,
                cancellationToken);

        var participantIds =
            dto.ParticipantIds
                .Distinct()
                .ToList();

        await ValidateParticipantIdsAsync(
            participantIds,
            cancellationToken);

        var meeting =
            _mapper.Map<Meeting>(
                dto);
        
        meeting.OrganizerId =
            request.OrganizerId;

        meeting.RoomId =
            room?.RoomId;

        meeting.MeetingParticipants =
            participantIds
                .Select(participantId =>
                    new MeetingParticipant
                    {
                        ParticipantId =
                            participantId
                    })
                .ToList();

        _context.Meetings.Add(
            meeting);

        await _context.SaveChangesAsync(
            cancellationToken);

        var createdMeeting =
            await _context.Meetings
                .AsNoTracking()
                .Include(item =>
                    item.Room)
                .Include(item =>
                    item.MeetingParticipants)
                .FirstAsync(
                    item =>
                        item.MeetingId ==
                        meeting.MeetingId,
                    cancellationToken);
        
        _logger.LogInformation(
            "Зустріч успішно створено. MeetingId: {MeetingId}, OrganizerId: {OrganizerId}, Title: {Title}",
            createdMeeting.MeetingId,
            request.OrganizerId,
            createdMeeting.Title);

        return _mapper.Map<MeetingReadDTO>(
            createdMeeting);
    }

    /// <summary>
    /// Повертає кімнату за її номером.
    /// </summary>
    private async Task<Room?> GetRoomAsync(
        int? roomNumber,
        CancellationToken cancellationToken)
    {
        if (!roomNumber.HasValue)
        {
            return null;
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
            throw new FluentValidation.ValidationException(
                $"Кімнату з номером {roomNumber.Value} не знайдено.");
        }

        return room;
    }

    /// <summary>
    /// Перевіряє існування всіх
    /// зазначених учасників.
    /// </summary>
    private async Task ValidateParticipantIdsAsync(
        List<int> participantIds,
        CancellationToken cancellationToken)
    {
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

        if (missingParticipantIds.Count == 0)
        {
            return;
        }

        throw new FluentValidation.ValidationException(
            $"Не знайдено учасників з ідентифікаторами: " +
            $"{string.Join(", ", missingParticipantIds)}.");
    }
}