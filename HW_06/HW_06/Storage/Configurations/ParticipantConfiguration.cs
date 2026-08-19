using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HW_06.Configurations;

/// <summary>
/// Конфігурація сутності учасника.
/// </summary>
public class ParticipantConfiguration
    : IEntityTypeConfiguration<Participant>
{
    /// <summary>
    /// Налаштовує поля, обмеження та зв'язки
    /// сутності учасника.
    /// </summary>
    /// <param name="builder">
    /// Побудовник конфігурації сутності учасника.
    /// </param>
    public void Configure(
        EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(x => x.ParticipantId);

        builder.Property(x => x.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Position)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.AvatarFileName)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.ApplicationUserId)
            .IsRequired();

        builder.HasOne(x => x.ApplicationUser)
            .WithOne(x => x.Participant)
            .HasForeignKey<Participant>(
                x => x.ApplicationUserId)
            .IsRequired();
    }
}