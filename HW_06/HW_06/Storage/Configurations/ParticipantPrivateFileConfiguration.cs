using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HW_06.Data.Configurations;

/// <summary>
/// Налаштовую сущність приватного файлу участника.
/// </summary>
public class ParticipantPrivateFileConfiguration
    : IEntityTypeConfiguration<ParticipantPrivateFile>
{
    /// <summary>
    /// Налаштовує структуру таблиці,
    /// властивості та зв'язки
    /// приватного файлу учасника.
    /// </summary>
    /// <param name="builder">
    /// Побудовник конфігурації сутності.
    /// </param>
    public void Configure(
        EntityTypeBuilder<ParticipantPrivateFile> builder)
    {
        builder.HasKey(file => file.Id);

        builder.Property(file => file.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(file => file.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(file => file.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(file => file.SizeBytes)
            .IsRequired();

        builder.Property(file => file.UploadedAtUtc)
            .IsRequired();

        builder.HasOne(file => file.SenderParticipant)
            .WithMany(participant => participant.SentPrivateFiles)
            .HasForeignKey(file => file.SenderParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(file => file.RecipientParticipant)
            .WithMany(participant => participant.ReceivedPrivateFiles)
            .HasForeignKey(file => file.RecipientParticipantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}