using Bogus;
using HW_06.Models;

namespace HW_06.Helpers;

public static class SeedData
{
    public static void Initialize(DataContext context)
    {
        if (context.Meetings.Any())
            return;

        // ---------------- Комнаты ----------------

        var rooms = new Faker<Room>()
            .RuleFor(r => r.NumberRoom, f => f.Random.Int(100, 120))
            .Generate(10);

        context.Rooms.AddRange(rooms);
        context.SaveChanges();

        // ---------------- Участники ----------------

        var roles = new[]
        {
            "Developer",
            "Manager",
            "Tester",
            "Designer",
            "Team Lead"
        };

        var participants = new Faker<Participant>()
            .RuleFor(p => p.FirstName, f => f.Person.FirstName)
            .RuleFor(p => p.LastName, f => f.Person.LastName)
            .RuleFor(p => p.Email, f => f.Person.Email)
            .RuleFor(p => p.Role, f => f.PickRandom(roles))
            .Generate(30);

        context.Participants.AddRange(participants);
        context.SaveChanges();

        // ---------------- Зустрічі ----------------

        var meetings = new Faker<Meeting>()
            .RuleFor(m => m.Title, f => f.Company.CatchPhrase())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.DateTime, f => f.Date.Future())
            .RuleFor(m => m.RoomId, f => f.PickRandom(rooms).RoomId)
            .Generate(20);

        context.Meetings.AddRange(meetings);
        context.SaveChanges();

        // ---------------- Зв'язки ----------------

        var meetingParticipants = new List<MeetingParticipant>();

        var random = new Random();

        foreach (var meeting in meetings)
        {
            var count = random.Next(5, 11); // від 5 до 10

            var selectedParticipants = participants
                .OrderBy(x => Guid.NewGuid())
                .Take(count);

            foreach (var participant in selectedParticipants)
            {
                meetingParticipants.Add(new MeetingParticipant
                {
                    MeetingId = meeting.MeetingId,
                    ParticipantId = participant.ParticipantId
                });
            }
        }

        context.MeetingParticipants.AddRange(meetingParticipants);
        context.SaveChanges();
    }
}