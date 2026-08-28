using Bogus;
using HW_06.Models;
using Microsoft.AspNetCore.Identity;
using HW_06.Common.Constants;

namespace HW_06.Helpers;

/// <summary>
/// Клас для початкового заповнення
/// бази даних тестовими даними.
/// </summary>
public static class SeedData
{
    private const string DefaultPassword = "Test123";

    /// <summary>
    /// Створює початкові ролі,
    /// користувачів, учасників, кімнати,
    /// зустрічі та зв'язки між ними.
    /// </summary>
    /// <param name="context">
    /// Контекст бази даних.
    /// </param>
    /// <param name="userManager">
    /// Менеджер користувачів ASP.NET Identity.
    /// </param>
    /// <param name="roleManager">
    /// Менеджер ролей ASP.NET Identity.
    /// </param>
    public static async Task InitializeAsync(
        DataContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);

        if (context.Meetings.Any())
        {
            return;
        }

        // ---------------- Ролі ----------------

        if (!await roleManager.RoleExistsAsync(
                ApplicationRoles.Admin))
        {
            await roleManager.CreateAsync(
                new IdentityRole(
                    ApplicationRoles.Admin));
        }

        if (!await roleManager.RoleExistsAsync(
                ApplicationRoles.User))
        {
            await roleManager.CreateAsync(
                new IdentityRole(
                    ApplicationRoles.User));
        }

        // ---------------- Кімнати ----------------

        var rooms = new Faker<Room>()
            .RuleFor(
                room => room.NumberRoom,
                faker => faker.Random.Int(100, 120))
            .Generate(10);

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        // ---------------- Користувачі та учасники ----------------
        

        var participants =
            new List<Participant>();

        var faker = new Faker();

        for (var index = 0; index < 30; index++)
        {
            var person = new Person();

            var firstName =
                person.FirstName;

            var lastName =
                person.LastName;

            var email =
                faker.Internet.Email(
                    firstName,
                    lastName);

            var applicationUser =
                new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

            var createUserResult =
                await userManager.CreateAsync(
                    applicationUser,
                    DefaultPassword);

            if (!createUserResult.Succeeded)
            {
                continue;
            }

            var roleName =
                participants.Count == 0
                    ? ApplicationRoles.Admin
                    : ApplicationRoles.User;

            var roleResult =
                await userManager.AddToRoleAsync(
                    applicationUser,
                    roleName);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(
                    applicationUser);

                continue;
            }

            var participant =
                new Participant
                {
                    FirstName =
                        firstName,

                    LastName =
                        lastName,

                    Position =
                        faker.PickRandom(
                            ParticipantPositions.All),

                    ApplicationUserId =
                        applicationUser.Id
                };

            participants.Add(
                participant);
        }

        context.Participants.AddRange(
            participants);

        await context.SaveChangesAsync();

        // ---------------- Зустрічі ----------------

        var meetings =
            new List<Meeting>();

        for (var index = 0; index < 20; index++)
        {
            var organizer =
                faker.PickRandom(
                    participants);

            var meeting =
                new Meeting
                {
                    Title =
                        faker.Company.CatchPhrase(),

                    Description =
                        faker.Lorem.Sentence(),

                    DateTime =
                        faker.Date.Future(),

                    RoomId =
                        faker.PickRandom(
                            rooms).RoomId,

                    OrganizerId =
                        organizer.ApplicationUserId!
                };

            meetings.Add(
                meeting);
        }

        context.Meetings.AddRange(
            meetings);

        await context.SaveChangesAsync();

        // ---------------- Зв'язки учасників із зустрічами ----------------

        var meetingParticipants =
            new List<MeetingParticipant>();

        var random =
            new Random();

        foreach (var meeting in meetings)
        {
            var organizer =
                participants.First(
                    participant =>
                        participant.ApplicationUserId ==
                        meeting.OrganizerId);

            var count =
                random.Next(5, 11);

            var selectedParticipants =
                participants
                    .Where(participant =>
                        participant.ParticipantId !=
                        organizer.ParticipantId)
                    .OrderBy(_ =>
                        Guid.NewGuid())
                    .Take(count - 1)
                    .ToList();

            selectedParticipants.Add(
                organizer);

            foreach (var participant
                     in selectedParticipants)
            {
                meetingParticipants.Add(
                    new MeetingParticipant
                    {
                        MeetingId =
                            meeting.MeetingId,

                        ParticipantId =
                            participant.ParticipantId
                    });
            }
        }

        context.MeetingParticipants.AddRange(
            meetingParticipants);

        await context.SaveChangesAsync();
    }
}