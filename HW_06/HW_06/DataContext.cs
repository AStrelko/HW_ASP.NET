using HW_06.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_06;

/// <summary>
/// Контекст бази даних застосунку.
/// Містить набори сутностей та конфігурацію зв'язків між ними.
/// </summary>
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);

        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<MeetingParticipant>()
            .HasKey(p => new {p.MeetingId, p.ParticipantId});
        
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(p => p.Meeting)
            .WithMany(m => m.MeetingParticipants)
            .HasForeignKey(p => p.MeetingId);
        
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(p => p.Participant)
            .WithMany(m => m.MeetingParticipants)
            .HasForeignKey(p => p.ParticipantId);
        
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.Room)
            .WithMany(r => r.Meetings)
            .HasForeignKey(m => m.RoomId);
        
        
    }
}