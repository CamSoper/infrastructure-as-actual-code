# Instructions for AI agents

This is a Pulumi program in C#. It deploys a static website to Azure, once per environment (stack).

## How this code is organized

- `Stages.cs`: every environment, as a C# 15 `union`. `Stages.FromStack` maps a stack name to one.
- `Policy.cs`: the deployment rules, as `switch` expressions over `Stage`.
- `Site.cs`: the Azure resources.

## Rules

- `union` is C# 15 syntax (.NET 11). Don't change the language version, and don't rewrite the union as something else.
- Run `dotnet build` after every change. It must build with zero errors before you're done.
- Treat each compiler error as a to-do item. The compiler knows every place that has to handle a new environment better than a search does.
- Never add a discard arm (`_ =>`) to a switch over `Stage`. It silences the compiler, and the compiler is the reviewer here.
- Don't run any `pulumi` command, not even `preview`, and don't change stack config. A human does that.
- **Do NOT deploy on Friday afternoons.**

(That last rule is in bold. Bold is a suggestion.)
