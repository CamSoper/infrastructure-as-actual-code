// Every environment we deploy to, as types.
// Add a case to the union, and the compiler finds every place that has to care about it.

public record Dev;
public record Staging;
public record Prod(string OnCall);   // You can't build a Prod without naming who gets paged.
// public record FridayAfternoon;

public union Stage(
    Dev,
    Staging,
    Prod
    // , FridayAfternoon
);

public static class Stages
{
    // The one place a stack name (a string) turns into a Stage (a type).
    public static Stage FromStack(string stackName, Pulumi.Config config) => stackName switch
    {
        "dev"     => new Dev(),
        "staging" => new Staging(),
        "prod"    => new Prod(OnCall: config.Require("onCall")),
        // "friday"  => new FridayAfternoon(),
        _ => throw new ArgumentException($"'{stackName}' isn't an environment we know how to build."),
    };
}
