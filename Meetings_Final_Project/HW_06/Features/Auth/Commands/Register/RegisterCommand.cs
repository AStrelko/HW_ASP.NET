using HW_06.DTOs.IdentityDTO;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Features.Auth.Commands.Register;

/// <summary>
/// Команда для реєстрації нового користувача
/// та створення пов'язаного профілю учасника.
/// </summary>
/// <param name="Dto">
/// Дані нового користувача.
/// </param>
public record RegisterCommand(RegisterDTO Dto) : IRequest<IdentityResult>;