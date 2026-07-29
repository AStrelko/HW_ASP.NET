using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Конфігурація сутності учасника.
/// </summary>
public class ParticipantConfiguration
    : IEntityTypeConfiguration<Participant>
{
    /// <summary>
    /// Налаштовує поля та обмеження таблиці учасників.
    /// </summary>
    /// <param name="builder">
    /// Побудовник конфігурації сутності учасника.
    /// </param>
    public void Configure(
        EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(x => x.ParticipantId);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.AvatarFileName)
            .HasMaxLength(255)
            .IsRequired(false);
    }
}