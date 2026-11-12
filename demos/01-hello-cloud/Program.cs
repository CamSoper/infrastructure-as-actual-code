// Demo 1: Hello, cloud.
// A real Azure website, in plain C#. No YAML was harmed in the making of this demo.

using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

return await Pulumi.Deployment.RunAsync(() =>
{
    #region The infrastructure
    // A resource group: Azure's junk drawer.
    var group = new ResourceGroup("hello-cloud");

    // A storage account. Pulumi adds a random suffix to the name,
    // so we never fight anyone for "site" again.
    var storage = new StorageAccount("site", new()
    {
        ResourceGroupName = group.Name,
        Kind = Kind.StorageV2,
        Sku = new SkuArgs { Name = SkuName.Standard_LRS },
    });

    // Flip on static website hosting.
    var website = new StorageAccountStaticWebsite("website", new()
    {
        ResourceGroupName = group.Name,
        AccountName = storage.Name,
        IndexDocument = "index.html",
    });
    #endregion

    #region The content (it's just C#)
    // The launch crew. It's a C# collection, so loops, LINQ, and IntelliSense come free.
    // DEMO: ask chat who else belongs on the crew, add them here, then `pulumi up`.
    string[] crew =
    [
        "The compiler",
        "The type system",
        "IntelliSense",
        "NuGet",
        "Unit tests",
    ];

    var page = new Blob("index.html", new()
    {
        ResourceGroupName = group.Name,
        AccountName = storage.Name,
        ContainerName = website.ContainerName,
        ContentType = "text/html",
        Source = new StringAsset(Page.Render(crew)),
    });
    #endregion

    // Outputs: what we want back after deploying.
    return new Dictionary<string, object?>
    {
        ["url"] = storage.PrimaryEndpoints.Apply(endpoints => endpoints.Web),
    };
});
