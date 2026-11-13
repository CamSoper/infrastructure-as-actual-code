// Every environment we deploy to, as types.
// Add a case to the union, and the compiler finds every place that has to care about it.

public record Dev;
public record Staging;
public record Prod(string OnCall);   // You can't build a Prod without naming who gets paged.
// public record Preview(int PullRequest);   // one per pull request, e.g. stack "pr-1013"

public union Stage(
    Dev,
    Staging,
    Prod
    // , Preview
);

public static class Stages
{
    // The one place a stack name (a string) turns into a Stage (a type).
    public static Stage FromStack(string stackName, Pulumi.Config config) => stackName switch
    {
        "dev"     => new Dev(),
        "staging" => new Staging(),
        "prod"    => new Prod(OnCall: config.Require("onCall")),
        // var s when s.StartsWith("pr-") && int.TryParse(s[3..], out var pr) => new Preview(pr),
        _ => throw new ArgumentException($"'{stackName}' isn't an environment we know how to build."),
    };
}
