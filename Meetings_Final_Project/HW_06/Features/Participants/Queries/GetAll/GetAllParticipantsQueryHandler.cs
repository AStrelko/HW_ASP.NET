using AutoMapper;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.Queryable;
using HW_06.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Features.Participants.Queries.GetAll;

/// <summary>
/// Обробник запиту для отримання
/// списку учасників.
/// </summary>
public class GetAllParticipantsQueryHandler
    : IRequestHandler<
        GetAllParticipantsQuery,
        PagedResult<ParticipantReadDTO>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Ініціалізує обробник запиту
    /// для отримання списку учасників.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="mapper">
    /// Сервіс AutoMapper.
    /// </param>
    public GetAllParticipantsQueryHandler(
        DataContext context,
        IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(mapper);

        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Отримує сторінку учасників
    /// із застосуванням пошуку та сортування.
    /// </summary>
    public async Task<PagedResult<ParticipantReadDTO>> Handle(
        GetAllParticipantsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Participant> query =
            _context.Participants
                .AsNoTracking()
                .Include(participant =>
                    participant.ApplicationUser);

        query = query
            .ApplySearch(
                request.Parameters.SearchLastName)
            .ApplySorting(
                request.Parameters);

        return await query
            .ToPagedResultAsync<
                Participant,
                ParticipantReadDTO>(
                    request.Parameters.Page,
                    request.Parameters.PageSize,
                    _mapper);
    }
}