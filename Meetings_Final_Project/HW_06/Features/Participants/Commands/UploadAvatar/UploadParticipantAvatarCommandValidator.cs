using FluentValidation;
using HW_06.Features.Common.Files;
using HW_06.Storage.Configurations;
using Microsoft.Extensions.Options;

namespace HW_06.Features.Participants.Commands.UploadAvatar;

/// <summary>
/// Виконує перевірку команди
/// завантаження аватара учасника.
/// </summary>
public class UploadParticipantAvatarCommandValidator
    : AbstractValidator<
        UploadParticipantAvatarCommand>
{
    private readonly FileStorageOptions
        _fileStorageOptions;

    public UploadParticipantAvatarCommandValidator(
        IOptions<FileStorageOptions> fileStorageOptions)
    {
        ArgumentNullException.ThrowIfNull(
            fileStorageOptions);

        _fileStorageOptions =
            fileStorageOptions.Value;

        RuleFor(command =>
                command.ParticipantId)
            .GreaterThan(0)
            .WithMessage(
                "Ідентифікатор учасника повинен бути більшим за нуль.");

        RuleFor(command =>
                command.File)
            .Custom((file, context) =>
            {
                var validationError =
                    AvatarFileValidator.ValidateAvatar(
                        file,
                        _fileStorageOptions
                            .MaxAvatarSizeBytes);

                if (validationError is not null)
                {
                    context.AddFailure(
                        validationError);
                }
            });
    }
}