// AGENTS.md says "Do NOT deploy on Friday afternoons." In bold.
// This file is that rule, minus the bold, plus the enforcement.

using Pulumi;

public static class FridayRule
{
    // Swappable, so the tests (and a Thursday rehearsal) can pretend it's any day they like.
    public static TimeProvider Clock { get; set; } = TimeProvider.System;

    // .NET Conf runs on Pacific time, so that's the clock that matters.
    static readonly TimeZoneInfo Pacific = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

    public static bool IsFridayAfternoon()
    {
        var now = TimeZoneInfo.ConvertTime(Clock.GetUtcNow(), Pacific);
        return now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 12;
    }

    // On a Friday afternoon, every stack is the FridayAfternoon stack. Unless someone signs for it:
    //   pulumi up --config blame=YourName
    public static Stage Apply(Stage stage, Config config) =>
        IsFridayAfternoon() && config.Get("blame") is null ? new FridayAfternoon() : stage;

    public static string? Footer(Config config) =>
        IsFridayAfternoon() && config.Get("blame") is { } who
            ? $"Shipped on a Friday afternoon. Blame: {who}."
            : null;
}
