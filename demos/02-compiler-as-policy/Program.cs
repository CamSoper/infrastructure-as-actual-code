// Demo 2: The compiler is your policy engine.
// Same website as demo 1, but now every environment is a type, and the rules are switch expressions.
// The stack lives in Site.cs; the environments in Stages.cs; the rules in Policy.cs.

return await Pulumi.Deployment.RunAsync(Site.Build);
