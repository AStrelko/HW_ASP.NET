using AutoMapper;
using AutoMapper.QueryableExtensions;
using HW_06.DTOs.Meeting;
using HW_06.DTOs.Participant;
using HW_06.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_06.Services;

/// <summary>
/// Сервіс для роботи з учасниками.
/// Забезпечує отримання, створення, оновлення та видалення учасників,
/// а також отримання списку зустрічей конкретного учасника.
/// </summary>
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
    
    /// <summary>
    /// Отримує список усіх учасників.
    /// </summary>
    /// <returns>Список учасників.</returns>
    public async Task<List<ParticipantReadDTO>> GetParticipants()
    {
        return await _context.Participants
            .AsNoTracking()
            .ProjectTo<ParticipantReadDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
    
    /// <summary>
    /// Отримує інформацію про учасника за його ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <returns>Інформація про учасника або null, якщо його не знайдено.</returns>
    public async Task<ParticipantReadDTO?> GetById(int id)
    {
        return await _context.Participants
            .AsNoTracking()
            .Where(p => p.ParticipantId == id)
            .ProjectTo<ParticipantReadDTO>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Створює нового учасника.
    /// </summary>
    /// <param name="dto">Дані нового учасника.</param>
    public async Task Create(ParticipantCreateDTO dto)
    {
        var participant = _mapper.Map<Participant>(dto);

        _context.Participants.Add(participant);

        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Повністю оновлює інформацію про учасника.
    /// </summary>
    /// <param name="dto">Нові дані учасника.</param>
    public async Task Update(ParticipantUpdateDTO dto)
    {
        var participant = _mapper.Map<Participant>(dto);

        _context.Participants.Update(participant);

        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Частково оновлює інформацію про учасника.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    /// <param name="dto">Поля, які необхідно оновити.</param>
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
    
    /// <summary>
    /// Отримує всі зустрічі, у яких бере участь вказаний учасник.
    /// </summary>
    /// <param name="participantId">Ідентифікатор учасника.</param>
    public async Task<List<MeetingreadDTO>> GetByParticipant(int participantId)
    {
        return await _context.Meetings
            .AsNoTracking()
            .Where(m => m.MeetingParticipants
                .Any(mp => mp.ParticipantId == participantId))
            .ProjectTo<MeetingreadDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
    
    /// <summary>
    /// Видаляє учасника за його ідентифікатором.
    /// </summary>
    /// <param name="id">Ідентифікатор учасника.</param>
    public async Task Delete(int id)
    {
        var participant = await _context.Participants.FindAsync(id);

        if (participant == null)
            return;

        _context.Participants.Remove(participant);

        await _context.SaveChangesAsync();
    }
}