using FluentValidation;

namespace HW_06.Features.Participants.Commands.ResetAvatar;

/// <summary>
/// Виконує перевірку команди
/// скидання аватара учасника.
/// </summary>
public class ResetParticipantAvatarCommandValidator
    : AbstractValidator<ResetParticipantAvatarCommand>
{
    public ResetParticipantAvatarCommandValidator()
    {
        RuleFor(command =>
                command.ParticipantId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");
    }
}