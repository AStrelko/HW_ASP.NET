using Bogus;
using HW_06.Models;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Helpers;

/// <summary>
/// Клас для заповнення бази даних тестовими даними.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(
        DataContext context,
        UserManager<ApplicationUser> userManager)
    {
        if (context.Meetings.Any())
            return;

        // ---------------- Комнати ----------------

        var rooms = new Faker<Room>()
            .RuleFor(r => r.NumberRoom, f => f.Random.Int(100, 120))
            .Generate(10);

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        // ---------------- Користувачі та учасники ----------------

        var positions = new[]
        {
            "Developer",
            "Manager",
            "Tester",
            "Designer",
            "Team Lead"
        };

        var participants = new List<Participant>();

        var faker = new Faker();

        for (var i = 0; i < 30; i++)
        {
            var person = new Person();

            var firstName = person.FirstName;
            var lastName = person.LastName;

            var email =
                faker.Internet.Email(firstName, lastName);

            var applicationUser = new ApplicationUser
            {
                Email = email,
                UserName = email
            };

            var result = await userManager.CreateAsync(
                applicationUser,
                "Test123");

            if (!result.Succeeded)
                continue;

            var participant = new Participant
            {
                FirstName = firstName,
                LastName = lastName,
                Position = faker.PickRandom(positions),
                ApplicationUserId = applicationUser.Id
            };

            participants.Add(participant);
        }

        context.Participants.AddRange(participants);
        await context.SaveChangesAsync();

        // ---------------- Зустрічі ----------------

        var meetings = new Faker<Meeting>()
            .RuleFor(m => m.Title, f => f.Company.CatchPhrase())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.DateTime, f => f.Date.Future())
            .RuleFor(m => m.RoomId, f => f.PickRandom(rooms).RoomId)
            .Generate(20);

        context.Meetings.AddRange(meetings);
        await context.SaveChangesAsync();

        // ---------------- Зв'язки ----------------

        var meetingParticipants = new List<MeetingParticipant>();

        var random = new Random();

        foreach (var meeting in meetings)
        {
            var count = random.Next(5, 11);

            var selectedParticipants = participants
                .OrderBy(_ => Guid.NewGuid())
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
        await context.SaveChangesAsync();
    }
}