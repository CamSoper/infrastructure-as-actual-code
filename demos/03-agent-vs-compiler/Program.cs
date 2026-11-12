// Demo 3: Agent vs. compiler.
// Demo 2's code, plus instructions for an AI agent (AGENTS.md). We ask the agent for a new environment
// and let the compiler review its work.
// The stack lives in Site.cs; the environments in Stages.cs; the rules in Policy.cs.

return await Pulumi.Deployment.RunAsync(Site.Build);
