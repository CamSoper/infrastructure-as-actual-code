// The whole stack is just a method. Which means you can call it from a unit test.

using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

public static class Site
{
    public static Dictionary<string, object?> Build()
    {
        var stackName = Pulumi.Deployment.Instance.StackName;
        var config = new Config();
        Stage stage = Stages.FromStack(stackName, config);
        // Policy.CheckDeployWindow(stage);

        var banner = Policy.Banner(stage);
        var tags = Policy.Tags(stage, stackName);

        var group = new ResourceGroup($"policy-{stackName}", new() { Tags = tags });

        var storage = new StorageAccount("site", new()
        {
            ResourceGroupName = group.Name,
            Kind = Kind.StorageV2,
            Sku = new SkuArgs { Name = Policy.Redundancy(stage) },
            Tags = tags,
        }, new CustomResourceOptions { Protect = Policy.IsProtected(stage) });

        var website = new StorageAccountStaticWebsite("website", new()
        {
            ResourceGroupName = group.Name,
            AccountName = storage.Name,
            IndexDocument = "index.html",
        });

        string[] crew = ["The compiler", "The type system", "IntelliSense", "NuGet", "Unit tests"];

        var page = new Blob("index.html", new()
        {
            ResourceGroupName = group.Name,
            AccountName = storage.Name,
            ContainerName = website.ContainerName,
            ContentType = "text/html",
            Source = new StringAsset(Page.Render(banner, crew)),
        });

        return new Dictionary<string, object?>
        {
            ["url"] = storage.PrimaryEndpoints.Apply(endpoints => endpoints.Web),
            ["redundancy"] = Policy.Redundancy(stage).ToString(),
            ["protected"] = Policy.IsProtected(stage),
        };
    }
}
