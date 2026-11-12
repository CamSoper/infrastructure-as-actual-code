// Policy as tests: the whole Azure stack runs in memory against a fake cloud.
// No Azure account, no network, no bill. Run with `dotnet test`.

using System.Collections.Immutable;
using Pulumi;
using Pulumi.Testing;
using Pulumi.AzureNative.Storage;
using Xunit;

// Pulumi's test runner uses process-wide state, so these run one at a time.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

class FakeCloud : IMocks
{
    public Task<(string? id, object state)> NewResourceAsync(MockResourceArgs args)
    {
        var state = args.Inputs;
        // Pretend Azure handed back a website URL, like the real thing does.
        if (args.Type == "azure-native:storage:StorageAccount")
        {
            state = state.Add("primaryEndpoints", new Dictionary<string, object>
            {
                ["web"] = $"https://{args.Name}.z19.web.core.windows.net/",
            });
        }
        return Task.FromResult<(string?, object)>(($"{args.Name}-id", state));
    }

    public Task<object> CallAsync(MockCallArgs args) =>
        Task.FromResult<object>(args.Args);
}

// A clock that says whatever we tell it to.
class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

public class StackTests : IDisposable
{
    static readonly DateTimeOffset TuesdayMorning = new(2026, 11, 10, 9, 0, 0, TimeSpan.FromHours(-8));
    static readonly DateTimeOffset FridayAfternoon = new(2026, 11, 13, 14, 30, 0, TimeSpan.FromHours(-8));

    // Every test starts on a Tuesday morning, so the suite passes even if you run it on a Friday afternoon.
    public StackTests() => FridayRule.Clock = new FixedClock(TuesdayMorning);
    public void Dispose() => FridayRule.Clock = TimeProvider.System;

    static async Task<ImmutableArray<Resource>> Deploy(string stack, string? blame = null)
    {
        Environment.SetEnvironmentVariable("PULUMI_CONFIG", blame is null
            ? """{ "agent-vs-compiler:onCall": "Cam" }"""
            : $$"""{ "agent-vs-compiler:onCall": "Cam", "agent-vs-compiler:blame": "{{blame}}" }""");
        var (resources, _) = await Pulumi.Deployment.TestAsync(
            new FakeCloud(),
            new TestOptions { ProjectName = "agent-vs-compiler", StackName = stack, IsPreview = false },
            Site.Build);
        return resources;
    }

    static Task<T> Value<T>(Output<T> output)
    {
        var done = new TaskCompletionSource<T>();
        output.Apply(v => { done.SetResult(v); return v; });
        return done.Task;
    }

    [Fact]
    public async Task Prod_is_zone_redundant()
    {
        var resources = await Deploy("prod");
        var storage = resources.OfType<StorageAccount>().Single();
        var sku = await Value(storage.Sku);
        Assert.Equal("Standard_ZRS", sku.Name);
    }

    [Fact]
    public async Task Prod_names_who_is_on_call()
    {
        var resources = await Deploy("prod");
        var storage = resources.OfType<StorageAccount>().Single();
        var tags = await Value(storage.Tags);
        Assert.Equal("Cam", tags!["on-call"]);
    }

    [Fact]
    public async Task Dev_is_cheap()
    {
        var resources = await Deploy("dev");
        var storage = resources.OfType<StorageAccount>().Single();
        var sku = await Value(storage.Sku);
        Assert.Equal("Standard_LRS", sku.Name);
    }

    [Fact]
    public async Task Nobody_deploys_on_friday_afternoon()
    {
        var ex = await Record.ExceptionAsync(() => Deploy("friday"));
        Assert.NotNull(ex);
        Assert.Contains("Close the laptop", ex.ToString());
    }

    [Fact]
    public async Task Friday_afternoon_blocks_every_stack()
    {
        FridayRule.Clock = new FixedClock(FridayAfternoon);
        var ex = await Record.ExceptionAsync(() => Deploy("dev"));
        Assert.NotNull(ex);
        Assert.Contains("Close the laptop", ex.ToString());
    }

    [Fact]
    public async Task Signing_for_it_lets_you_deploy_anyway()
    {
        FridayRule.Clock = new FixedClock(FridayAfternoon);
        var resources = await Deploy("dev", blame: "Cam");
        Assert.Single(resources.OfType<StorageAccount>());
    }

    // The rules are plain functions too, so you can test them without Pulumi at all.
    [Fact]
    public void Prod_and_friday_are_protected()
    {
        Assert.True(Policy.IsProtected(new Prod(OnCall: "Cam")));
        Assert.True(Policy.IsProtected(new FridayAfternoon()));
        Assert.False(Policy.IsProtected(new Staging()));
        Assert.False(Policy.IsProtected(new Dev()));
    }
}
