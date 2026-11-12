# Infrastructure as (Actual) Code with C# 15 and .NET 11

Slides, demos, and speaker notes from my [.NET Conf 2026](https://www.dotnetconf.net) Community Days talk.

> Infrastructure as code usually means infrastructure as YAML, or a DSL that's almost a programming language.
> But if you already know C#, you already know how to build cloud infrastructure.

Everything here is open source. You don't need a Pulumi account: `pulumi login --local` keeps state on your machine.

## What's in here

| Folder | What it is | Needs Azure? |
|---|---|---|
| [`slides/`](slides/) | The deck (PowerPoint and PDF) and the full speaker notes as Markdown | No |
| [`demos/01-hello-cloud`](demos/01-hello-cloud/) | A static website on Azure, in plain C# | Yes |
| [`demos/02-compiler-as-policy`](demos/02-compiler-as-policy/) | Environments as C# 15 union types, so a missing rule is a build error | Only to deploy |
| [`demos/03-agent-vs-compiler`](demos/03-agent-vs-compiler/) | An AI agent adds an environment, and the compiler reviews its work | Only to deploy |
| [`tests/StackTests`](tests/StackTests/) | The whole stack, unit-tested against a fake cloud | No |
| [`bonus/single-file`](bonus/single-file/) | The whole thing in one file: `dotnet run deploy.cs` | Yes |
| [`docs/run-of-show.md`](docs/run-of-show.md) | My presenter cue cards, timings, and fallbacks | No |

## Try it without a cloud account

You only need the [.NET 11 SDK](https://dotnet.microsoft.com/download).

```bash
# See the compiler enforce the rules
cd demos/02-compiler-as-policy
dotnet build

# Now uncomment the FridayAfternoon lines in Stages.cs (just the record and the union member),
# build again, and read the errors. Then uncomment the rest and build once more.

# Run the whole Azure stack against a fake cloud (including a fake Friday)
cd ../../tests/StackTests
dotnet test
```

## Deploy it for real

You'll also need:

- The [Pulumi CLI](https://www.pulumi.com/docs/iac/download-install/) and the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli).
- An Azure subscription where you can create resource groups (Contributor is enough), with the `Microsoft.Storage` provider registered. It usually is; if not, run `az provider register --namespace Microsoft.Storage`.
- bash, only if you want the `scripts/` helpers. Linux and macOS have it; on Windows, use [WSL](https://learn.microsoft.com/windows/wsl/install) and install the tools above inside it.

```bash
az login
az account set --subscription "<the one you want billed>"

pulumi login --local            # state stays on your machine. Or use Pulumi Cloud, or a blob container.
export PULUMI_CONFIG_PASSPHRASE="pick-something"   # local logins encrypt secrets with this; Pulumi prompts if it's unset

cd demos/01-hello-cloud
pulumi stack init dev
pulumi up                       # prints the website URL when it's done
```

Everything deploys to `centralus`. To use another region, `pulumi config set azure-native:location westus3` (per stack) or edit `Pulumi.yaml`.

| Demo | Stacks you can create | What you get |
|---|---|---|
| 01 | anything, e.g. `dev` | The launch-poll site |
| 02 | `dev`, `staging`, `prod`, `friday` | A banner per environment. `prod` is zone-redundant and protected. `friday` refuses to deploy. |
| 03 | the same, plus `pr-<number>` once Preview exists | As demo 2, and demo 3 has one more rule (see below). |

Create each stack before its first deploy with `pulumi stack init <name>` (or `pulumi up --stack <name>` and say yes when it offers to create it).

Demo 3 refuses to deploy on Friday afternoons, Pacific time. Add `--config blame=<you>` to override. That's a feature.

`prod` needs to know who's on call; `Pulumi.prod.yaml` already says it's me. Change it with `pulumi config set onCall <you> --stack prod`.

### Cleaning up

Each demo creates one resource group, one storage account, and one small file. That costs pennies a month, but clean up when you're done:

```bash
pulumi destroy --stack dev
pulumi stack rm dev
```

The `prod` stacks are protected on purpose: `pulumi destroy` refuses until you run `pulumi state unprotect --all --stack prod`. That's the point.

Or let `scripts/cleanup.sh` tear down every stack in every demo, protected ones included. Then check the Azure portal for resource groups starting with `hello-cloud`, `policy-`, `agent-`, or `single-file-`.

## The demos

### 1. Hello, cloud

A resource group, a storage account with static website hosting, and an `index.html` built in C#. The page is a go/no-go launch poll, written with LINQ and a raw string literal. Everyone is GO except YAML.

### 2. The compiler is your policy engine

Every environment is a type:

```csharp
public record Dev;
public record Staging;
public record Prod(string OnCall);   // You can't build a Prod without naming who gets paged.

public union Stage(Dev, Staging, Prod);
```

The deployment rules (redundancy, delete protection, the site banner) are `switch` expressions with no default arm. One line in the `.csproj` makes a missed case a build error:

```xml
<WarningsAsErrors>$(WarningsAsErrors);CS8509</WarningsAsErrors>
```

In the talk, we add a `FridayAfternoon` environment and let the compiler find every rule that has to deal with it. The lines are already in the files, commented out, so you can do it too.

### 3. Agent vs. compiler

Demo 2's end state, plus an [`AGENTS.md`](demos/03-agent-vs-compiler/AGENTS.md) that says, in bold, "Do NOT deploy on Friday afternoons."

1. In `Stages.cs`, uncomment the `Preview` record and the `, Preview` line in the union. `dotnet build` fails with three errors.
2. Hand your agent (any agent) the [prompt](demos/03-agent-vs-compiler/prompts/preview-environments.md): "I added a `Preview` environment and the build is broken. Fix it." The compiler's errors are its to-do list.
3. Review the diff, build it yourself, then `pulumi stack init pr-1013` and `pulumi up --stack pr-1013`.

If you do step 3 on a Friday afternoon (Pacific time), find out what [`FridayRule.cs`](demos/03-agent-vs-compiler/FridayRule.cs) does. Bold text is a suggestion. Code isn't.

No agent handy? Apply the reference change instead of step 2:

```bash
cd demos/03-agent-vs-compiler
git checkout -- .
git apply --ignore-whitespace fallback/preview-environment.patch
dotnet build
```

### Bonus: one file

[`bonus/single-file/deploy.cs`](bonus/single-file/deploy.cs) uses Pulumi's Automation API, so the program *is* the deployment tool:

```bash
cd bonus/single-file
dotnet run deploy.cs -- preview dev
dotnet run deploy.cs -- up dev
dotnet run deploy.cs -- destroy dev
```

One gotcha: file-based apps set `PublishAot=true` by default, which turns off reflection-based JSON, and Pulumi needs it. The file opts out with `#:property PublishAot=false`. Without it you get a runtime error instead of a deployment. The first `up` may also download the Azure provider plugin, so give it a minute.

## Versions

Built and tested with the .NET 11.0.100 SDK, C# 15, Pulumi .NET SDK 3.118.0, and Pulumi Azure Native 3.30.0. Pulumi's docs list .NET 8, 9, and 10 as tested targets; everything here builds and its tests pass on .NET 11.

## Learn more

- [Pulumi for .NET](https://www.pulumi.com/docs/iac/languages-sdks/dotnet/)
- [Unit testing Pulumi programs](https://www.pulumi.com/docs/iac/guides/testing/unit/)
- [What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
- [File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)

## License

[MIT](LICENSE)
