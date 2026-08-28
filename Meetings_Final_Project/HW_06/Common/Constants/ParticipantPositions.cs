namespace HW_06.Common.Constants;

/// <summary>
/// Містить доступні посади
/// або спеціалізації учасників.
/// </summary>
public static class ParticipantPositions
{
    public const string Developer =
        "Developer";

    public const string Manager =
        "Manager";

    public const string Tester =
        "Tester";

    public const string Designer =
        "Designer";

    public const string TeamLead =
        "Team Lead";

    /// <summary>
    /// Усі доступні посади учасників.
    /// </summary>
    public static readonly string[] All =
    [
        Developer,
        Manager,
        Tester,
        Designer,
        TeamLead
    ];
}