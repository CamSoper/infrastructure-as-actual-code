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
        // FridayAfternoon => SkuName.Standard_RAGZRS,   // maximum redundancy, maximum anxiety
    };

    // Protected resources can't be deleted until someone deliberately unprotects them.
    public static bool IsProtected(Stage stage) => stage switch
    {
        Dev     => false,
        Staging => false,
        Prod    => true,
        // FridayAfternoon => true,                      // nobody touches anything
    };

    // What the website says about where you are.
    public static Banner Banner(Stage stage) => stage switch
    {
        Dev     => new("🚧 Dev: nothing here is real", "#f7b548"),
        Staging => new("🧪 Staging: almost real", "#28c2d1"),
        Prod p  => new($"✅ Production: {p.OnCall} is on call, be nice", "#28c2d1"),
        // FridayAfternoon => throw new GoHomeException(),
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
}

public record Banner(string Text, string Color);

// class GoHomeException() : Exception("It's Friday afternoon. The deploy can wait. Close the laptop.");
