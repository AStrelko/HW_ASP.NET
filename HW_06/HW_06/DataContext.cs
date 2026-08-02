using HW_06.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_06;

/// <summary>
/// Представляє контекст бази даних застосунку
/// та надає доступ до його сутностей.
/// </summary>
public class DataContext : DbContext
{
    /// <summary>
    /// Ініціалізує новий екземпляр
    /// контексту бази даних.
    /// </summary>
    /// <param name="options">
    /// Параметри конфігурації контексту.
    /// </param>
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Набір зустрічей.
    /// </summary>
    public DbSet<Meeting> Meetings { get; set; }
    /// <summary>
    /// Набір кімнат.
    /// </summary>
    public DbSet<Room> Rooms { get; set; }
    /// <summary>
    /// Набір учасників.
    /// </summary>
    public DbSet<Participant> Participants { get; set; }
    /// <summary>
    /// Набір зв’язків між зустрічами
    /// та учасниками.
    /// </summary>
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    /// <summary>
    /// Набір публічних файлів-вкладень зустрічей.
    /// </summary>
    public DbSet<MeetingAttachment> MeetingAttachments { get; set; }
    /// <summary>
    /// Набір приватних файлів учасників.
    /// </summary>
    public DbSet<ParticipantPrivateFile> ParticipantPrivateFiles { get; set; }

    /// <summary>
    /// Налаштовує моделі сутностей,
    /// їх ключі та зв’язки.
    /// </summary>
    /// <param name="modelBuilder">
    /// Побудовник моделі Entity Framework Core.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Застосовує всі конфігурації сутностей, визначені в поточній збірці.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DataContext).Assembly);

        base.OnModelCreating(modelBuilder);

        // Налаштовує складений первинний ключ проміжної сутності.
        modelBuilder.Entity<MeetingParticipant>()
            .HasKey(link => new
            {
                link.MeetingId,
                link.ParticipantId
            });

        // Налаштовує зв’язок між зустріччю та проміжною сутністю.
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(link => link.Meeting)
            .WithMany(meeting =>
                meeting.MeetingParticipants)
            .HasForeignKey(link =>
                link.MeetingId);

        // Налаштовує зв’язок між учасником та проміжною сутністю.
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(link => link.Participant)
            .WithMany(participant =>
                participant.MeetingParticipants)
            .HasForeignKey(link =>
                link.ParticipantId);

        // Налаштовує зв’язок між зустріччю та кімнатою.
        modelBuilder.Entity<Meeting>()
            .HasOne(meeting => meeting.Room)
            .WithMany(room => room.Meetings)
            .HasForeignKey(meeting =>
                meeting.RoomId);
        
        
    }
}