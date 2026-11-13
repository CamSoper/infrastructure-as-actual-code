// Our deployment rules, written as code instead of a wiki page.
// Note what's missing: no `_ =>` default arms. A discard is you telling the compiler to stop helping.

using Pulumi.AzureNative.Storage;

public static class Policy
{
    // How many copies of our data Azure keeps.
    public static SkuName Redundancy(Stage stage) => stage switch
    {
        Dev     => SkuName.Standard_LRS,   // locally redundant: one datacenter
        Staging => SkuName.Standard_LRS,
        Prod    => SkuName.Standard_ZRS,   // zone redundant: three datacenters
        // Preview => SkuName.Standard_LRS,   // throwaway, so cheap
    };

    // Protected resources can't be deleted until someone deliberately unprotects them.
    public static bool IsProtected(Stage stage) => stage switch
    {
        Dev     => false,
        Staging => false,
        Prod    => true,
        // Preview => false,                  // previews get deleted; don't make that hard
    };

    // What the website says about where you are.
    public static Banner Banner(Stage stage) => stage switch
    {
        Dev     => new("🚧 Dev: nothing here is real", "#f7b548"),
        Staging => new("🧪 Staging: almost real", "#28c2d1"),
        Prod p  => new($"✅ Production: {p.OnCall} is on call, be nice", "#28c2d1"),
        // Preview p => new($"🔍 Preview of pull request #{p.PullRequest}", "#b9aaee"),
    };

    public static Dictionary<string, string> Tags(Stage stage, string stackName)
    {
        var tags = new Dictionary<string, string>
        {
            ["environment"] = stackName,
            ["managed-by"] = "pulumi",
            ["lines-of-yaml"] = "0",
        };
        if (stage is Prod prod)
        {
            tags["on-call"] = prod.OnCall;
        }
        return tags;
    }

    // Everybody knows the rule. Now it's code.
    public static void CheckDeployWindow(Stage stage)
    {
        if (stage is Prod && IsFridayAfternoon())
        {
            throw new GoHomeException();
        }
    }

    // .NET Conf runs on Pacific time, so that's the clock that matters.
    static bool IsFridayAfternoon() =>
        TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"))
            is { DayOfWeek: DayOfWeek.Friday, Hour: >= 12 };
}

public record Banner(string Text, string Color);

class GoHomeException() : Exception("No prod deploys on Friday afternoon. The deploy can wait. Close the laptop.");
