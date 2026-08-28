using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HW_06.Configurations;

/// <summary>
/// Настраивает сущность <see cref="MeetingAttachment"/>
/// и её связь с сущностью <see cref="Meeting"/>.
/// </summary>
public class MeetingAttachmentConfiguration
    : IEntityTypeConfiguration<MeetingAttachment>
{
    /// <summary>
    /// Выполняет настройку таблицы, свойств и связей
    /// для сущности файла-вложения встречи.
    /// </summary>
    /// <param name="builder">
    /// Объект построителя конфигурации сущности.
    /// </param>
    public void Configure(EntityTypeBuilder<MeetingAttachment> builder)
    {
        builder.HasKey(attachment => attachment.Id);

        builder.Property(attachment => attachment.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(attachment => attachment.StoredFileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(attachment => attachment.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(attachment => attachment.SizeBytes)
            .IsRequired();

        builder.Property(attachment => attachment.UploadedAtUtc)
            .IsRequired();

        builder.HasOne(attachment => attachment.Meeting)
            .WithMany(meeting => meeting.Attachments)
            .HasForeignKey(attachment => attachment.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}