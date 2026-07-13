using AutoMapper;
using AutoMapper.QueryableExtensions;
using HW_06.DTOs.Participant;
using HW_06.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

public class ParticipantDTOService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public ParticipantDTOService(
        DataContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    // Отримання списку всіх учасників
    public async Task<List<ParticipantReadDTO>> GetParticipants()
    {
        return await _context.Participants
            .AsNoTracking()
            .ProjectTo<ParticipantReadDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
    
    // Отримання учасника за id
    public async Task<ParticipantReadDTO?> GetById(int id)
    {
        return await _context.Participants
            .AsNoTracking()
            .Where(p => p.ParticipantId == id)
            .ProjectTo<ParticipantReadDTO>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
    
    // Створення нового учасника
    public async Task Create(ParticipantCreateDTO dto)
    {
        var participant = _mapper.Map<Participant>(dto);

        _context.Participants.Add(participant);

        await _context.SaveChangesAsync();
    }
    
    // Повне оновлення учасника
    public async Task Update(ParticipantUpdateDTO dto)
    {
        var participant = _mapper.Map<Participant>(dto);

        _context.Participants.Update(participant);

        await _context.SaveChangesAsync();
    }
    
    // Часткове оновлення учасника
    public async Task PartialUpdate(int id, ParticipantPartialUpdateDTO dto)
    {
        var participant = await _context.Participants.FindAsync(id);

        if (participant == null)
            return;

        if (dto.FirstName != null)
            participant.FirstName = dto.FirstName;

        if (dto.LastName != null)
            participant.LastName = dto.LastName;

        if (dto.Email != null)
            participant.Email = dto.Email;

        if (dto.Role != null)
            participant.Role = dto.Role;

        await _context.SaveChangesAsync();
    }
    
    // Видалення учасника за id
    public async Task Delete(int id)
    {
        var participant = await _context.Participants.FindAsync(id);

        if (participant == null)
            return;

        _context.Participants.Remove(participant);

        await _context.SaveChangesAsync();
    }
}