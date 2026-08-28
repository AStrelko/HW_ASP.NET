using HW_06.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Конфігурація моделі <see cref="Meeting"/>
/// для Entity Framework Core.
/// </summary>
public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    /// <summary>
    /// Налаштовує структуру таблиці та властивостей
    /// сутності <see cref="Meeting"/>.
    /// </summary>
    /// <param name="builder">
    /// Будівник конфігурації сутності.
    /// </param>
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasKey(x => x.MeetingId);

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);
        
        builder.Property(x => x.OrganizerId)
            .HasMaxLength(450)
            .IsRequired();
    }
}