#!/usr/bin/env dotnet
// Bonus: the whole thing in one file. No .csproj, no Pulumi.yaml.
//
//   dotnet run deploy.cs -- preview dev
//   dotnet run deploy.cs -- up dev
//   dotnet run deploy.cs -- destroy dev
//
// You still need the Pulumi CLI installed and logged in (`pulumi login`), plus Azure credentials (`az login`).
// The Automation API turns Pulumi into a library: this program *is* the deployment tool.

#:package Pulumi.Automation@3.118.0
#:package Pulumi.AzureNative@3.30.0
#:property PublishAot=false
// ^ The gotcha: file-based apps default to Native AOT, which turns off reflection-based JSON.
//   Pulumi's SDK needs it, so without this line you get a runtime error, not a deploy.

using Pulumi;
using Pulumi.Automation;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

var verb = args.ElementAtOrDefault(0) ?? "preview";
var stackName = args.ElementAtOrDefault(1) ?? "dev";

Stage stage = stackName switch
{
    "dev"  => new Dev(),
    "prod" => new Prod(OnCall: Environment.UserName),   // Whoever runs the prod deploy is on call. Seems fair.
    _      => throw new ArgumentException($"'{stackName}' isn't an environment we know how to build."),
};

// The infrastructure: the same code you'd put in Program.cs, inside a lambda.
var program = PulumiFn.Create(() =>
{
    var group = new ResourceGroup($"single-file-{stackName}");

    var storage = new StorageAccount("site", new()
    {
        ResourceGroupName = group.Name,
        Kind = Kind.StorageV2,
        Sku = new SkuArgs
        {
            Name = stage switch
            {
                Dev  => SkuName.Standard_LRS,
                Prod => SkuName.Standard_ZRS,
            },
        },
    });

    var website = new StorageAccountStaticWebsite("website", new()
    {
        ResourceGroupName = group.Name,
        AccountName = storage.Name,
        IndexDocument = "index.html",
    });

    var banner = stage switch
    {
        Dev    => "🚧 Dev, from a single C# file",
        Prod p => $"✅ Production, from a single C# file. {p.OnCall} is on call.",
    };

    new Blob("index.html", new()
    {
        ResourceGroupName = group.Name,
        AccountName = storage.Name,
        ContainerName = website.ContainerName,
        ContentType = "text/html",
        Source = new StringAsset($"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><title>One file</title></head>
            <body style="font-family: system-ui, sans-serif; background: #120838; color: #fff; padding: 2rem">
            <h1>{System.Net.WebUtility.HtmlEncode(banner)}</h1></body></html>
            """),
    });

    return new Dictionary<string, object?>
    {
        ["url"] = storage.PrimaryEndpoints.Apply(endpoints => endpoints.Web),
    };
});

// The driver: replaces typing `pulumi up` in a terminal.
var stack = await LocalWorkspace.CreateOrSelectStackAsync(new InlineProgramArgs("single-file", stackName, program));
await stack.SetConfigAsync("azure-native:location", new ConfigValue("centralus"));

switch (verb)
{
    case "up":
        var result = await stack.UpAsync(new() { OnStandardOutput = Console.WriteLine });
        Console.WriteLine($"\n🚀 {result.Outputs["url"].Value}");
        break;
    case "destroy":
        await stack.DestroyAsync(new() { OnStandardOutput = Console.WriteLine });
        await stack.Workspace.RemoveStackAsync(stackName);
        break;
    default:
        await stack.PreviewAsync(new() { OnStandardOutput = Console.WriteLine });
        break;
}

// In any program with top-level statements, type declarations go after the top-level code.
public record Dev;
public record Prod(string OnCall);
public union Stage(Dev, Prod);
