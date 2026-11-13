# Run of show

**Infrastructure as (Actual) Code with C# 15 and .NET 11**
.NET Conf 2026 Community Days, Friday, November 13, 2:30 to 3:00 PM PST (4:30 CT). StreamYard, 1920 x 1080.
Check in with the host 15 minutes early. Be back 5 minutes before start.

Target: about 21 minutes of content, then Q&A. Hard stop on content at 25:00. Start the timer on slide 2; slide 1 is the host intro.

**Checkpoints:** 0:45 lock slide, 1:05 versioning slide, 1:45 bio, 3:45 demo 1, 8:30 `pulumi up` in demo 1, 11:00 demo 2, 14:00 the Preview step, 16:00 the Friday rule, 18:00 hades story, 21:00 Q&A.

| Time | Slide(s) | What happens |
|---|---|---|
| 0:00 | 2 to 5 | Cold open: define IaC in one line, then Terraform, one "if" per slide (ternary, `count = prod ? 1 : 0`, `dynamic`), then "config with extra steps" |
| 1:45 | 6 | Who's this guy (30 seconds, open source disclaimer) |
| 2:15 | 7 to 8 | What "actual code" means; Pulumi in one slide |
| 3:45 | 9 | **Demo 1: Hello, cloud** |
| 10:00 | 10 | C# 15 unions in one slide |
| 11:00 | 11 | **Demo 2: The compiler is your policy engine** |
| 18:00 | 12 to 13 | The hades story; the thesis |
| 20:00 | 14 | Take it home: repo, tests, single file |
| 21:00 | 15 | Thanks, Q&A |

Demo 3 (agent vs. compiler) was cut from the live talk on 2026-11-13. Its code stays in the repo as a take-home.

If you're running long, cut in this order:

1. Demo 1, steps 7 to 9 (add a crew member and redeploy). Show the live site and move on.
2. Slide 12 (the hades story). Go straight to slide 13.
3. Demo 2, step 13 (`pulumi preview --stack prod`). Point at `CheckDeployWindow` instead.

**Never cut demo 2's Preview build errors.** They're the payoff.

---

## Demo 1: Hello, cloud (3:45 to 10:00)

**Folder:** `demos/01-hello-cloud` **Stack:** `dev`, already deployed

| # | Do | Say |
|---|---|---|
| 1 | Open `Pulumi.yaml`. | "The project file. A name, `runtime: dotnet`, and one setting: the Azure region. It's not the infrastructure; it tells Pulumi how to run my program." |
| 2 | Open `Program.cs`. Collapse both regions, then expand "The infrastructure". | "It's a console app. A `.csproj`, NuGet packages, `Program.cs`. That's the whole project." |
| 3 | Hover `SkuName.Standard_LRS`. | "IntelliSense for Azure. Every property, every allowed value, documented. LRS means locally redundant: one datacenter. Fine for a demo." |
| 4 | Point at `group.Name` in the storage account. | "That's a real reference. Pulumi works out the order from the code. No `dependsOn`." |
| 5 | Expand "The content". Open `Page.cs`. | "The website is a raw string literal. The go/no-go poll is LINQ." |
| 6 | Switch to the browser tab with the deployed site. | "This is live on Azure, right now." |
| 7 | Add `"Rubber duck"` to `crew` in `Program.cs` (new line after `"Unit tests"`, line 41). | "Who else belongs on the launch crew? The rubber duck. Every debugger's first crew member. It's user input, so it gets HTML-encoded." |
| 8 | `pulumi up` | "Preview first. It's diffing against what I deployed last time. One resource changes: the page gets replaced. Nothing else gets touched. If you've used Terraform, this is `terraform plan`." Select **yes**. |
| 9 | Refresh the browser. | "New crew member, GO for launch." |

**Land it:** "That's a loop, a string, and a list. In a real language. With a compiler watching."

**Fallback:** If Azure or Pulumi misbehaves, show the browser tab you opened before the talk and `git diff`. Say "the cloud is having a moment, here's the change," and move on.

---

## Demo 2: The compiler is your policy engine (11:00 to 18:00)

**Folder:** `demos/02-compiler-as-policy` **Stacks:** `dev` deployed; `prod` created, never deployed

Everything you need to type is already in the files, commented out. Use **Ctrl+K Ctrl+U** (Cmd+K Cmd+U on a Mac) to uncomment.

**What you're looking at**

- **The story:** environments are a closed list (a union). Every rule is a switch over that list. Add an environment and the compiler flags every rule that hasn't decided about it.
- **Stack vs. `Stage`:** a stack is one deployed copy of the program (same code, its own config and state). Pulumi gives you its name as a string. `Stage` is our type for what kind of environment that stack is, and `Site.Build` converts one to the other first thing.
- **`Stages.cs`:** `Dev` and `Staging` are empty marker records. `Prod` requires `OnCall`. `FromStack` is the one place a stack name (a string) becomes a `Stage` (a type). Its `_ =>` is fine, because strings are open-ended and the union isn't.
- **`Policy.cs`:** three switches, three rules. `Redundancy` picks the SKU, `IsProtected` sets delete protection, and `Banner` is what the site says. No `_ =>` anywhere, so the compiler checks that each one covers every `Stage`.
- **`.csproj`:** `WarningsAsErrors` with CS8509 ("switch doesn't handle every case") turns a warning into a failed build. That's the policy engine.
- **The 3 errors:** one per switch, at `Policy.cs` lines 9, 18, and 27.
- **The Friday rule:** `Policy.CheckDeployWindow` (line 51) throws `GoHomeException` for `Prod` when it really is Friday afternoon, Pacific time. `IsFridayAfternoon` (line 60) is a property pattern. Turning it on is one line in `Site.cs` (line 15). `pulumi preview --stack prod` runs the program, so it throws before anything is created.

| # | Do | Say |
|---|---|---|
| 1 | `Program.cs`. | "Same website as demo 1, split into files. Program.cs is one line: hand Pulumi the `Build` method." |
| 2 | F12 into `Site.Build`. Highlight line 12 (`StackName`), then line 14 (`Stages.FromStack`). Scroll past the resources; don't dwell. F12 into `FromStack`. | "Which stack am I deploying? A stack is one deployed copy of this program: same code, its own config, its own state. dev, prod, and soon friday. Pulumi hands me its name as a string, so first we turn it into a `Stage`: our own type for what kind of environment this stack is. Everything below asks the Stage, never the string." |
| 3 | `Stages.cs`: the records, the union, `FromStack`. | "Every environment is a type. And look: you can't make a `Prod` without naming who gets paged." |
| 4 | `Policy.cs`: three switches. | "Our deployment rules. Redundancy, delete protection, the banner. And notice what's missing: no `_ =>`. A discard arm is you telling the compiler to stop helping." |
| 5 | Back to `Site.cs`. Highlight line 26 (`Sku = Policy.Redundancy(stage)`), then line 28 (`Protect = Policy.IsProtected(stage)`). *Optional, if on time:* in `Policy.cs` line 11 change `Dev` to `Standard_GRS`, run `pulumi preview --stack dev`, point at the storage account's `sku` update, then Ctrl+Z. | "Here's where those rules land. Redundancy is this storage account's SKU. IsProtected is Pulumi's delete protection. Banner becomes the page. Change a rule, and the real infrastructure changes." |
| 6 | `CompilerAsPolicy.csproj`: the `WarningsAsErrors` line. | "CS8509 is the 'your switch missed a case' warning. This one line makes it an error. That's the difference between a policy and a suggestion." |
| 7 | Browser tab: the `dev` site's yellow banner. | Read it: "Dev: nothing here is real." |
| 8 | In `Stages.cs`, uncomment the `Preview` record (line 7) and `, Preview` in the union (line 13). | "Every team I know wants a preview environment per pull request. Let's add one." |
| 9 | `dotnet build` | You'll see 6 error lines (each is printed twice); point at the "3 Error(s)" summary. Read one aloud: "Policy.cs line 9: the switch doesn't handle Preview." Then: "Three errors. One per rule. The compiler just reviewed our entire deployment policy." Callback: "In Terraform, add an environment and nothing tells you. Here, three build errors." |
| 10 | In `Policy.cs`, uncomment the three `Preview` arms (lines 14, 23, 32). In `Stages.cs`, uncomment the `pr-` line (line 24). Read the comments. | "Throwaway, so cheap. Previews get deleted; don't make that hard." |
| 11 | `dotnet build` | "Green." |
| 12 | `Policy.cs`: scroll to `CheckDeployWindow` (line 51), then `IsFridayAfternoon` (line 60). | "Everybody knows the rule: never deploy prod on a Friday afternoon. Usually that's a wiki page. Here it's code. And look: `is { DayOfWeek: Friday, Hour: >= 12 }`. More pattern matching." |
| 13 | `Site.cs`: uncomment line 15 (`Policy.CheckDeployWindow(stage);`). Clear the terminal (Ctrl+L). `pulumi preview --stack prod` | Read the message: "No prod deploys on Friday afternoon. The deploy can wait. Close the laptop." Then, slowly: "It's Friday. It's the afternoon. Which is a joke. Probably." |

**Land it:** "The compiler didn't write the rules. It made me decide. And the rules that matter are code, not a wiki page."

**Fallback:** If `pulumi preview` is slow or broken, skip step 13 and just show `CheckDeployWindow`. If it doesn't throw, say "The rule says Friday afternoon, Pacific. Somebody check my clock." and move on.

**After the talk:** `git checkout -- .` in this folder to put the comments back.

---

## Prep (Thursday, and again Friday morning)

**WSL setup (once).** Everything runs inside WSL: the .NET 11 SDK, the Pulumi CLI, the Azure CLI all need installing there, not just on Windows. Clone the repo into the Linux filesystem (`~/src/...`), not `/mnt/c/...`, which is slow and invites CRLF trouble. Open it with `code .` from WSL so VS Code uses the WSL remote, and its terminal is bash. If `az login` in WSL can't open a browser, use `az login --use-device-code`.

Using a local Pulumi backend (`pulumi login --local`)? Set `PULUMI_CONFIG_PASSPHRASE` in the shell first, or every command stops to ask for it. On stream, that's a password prompt on screen.

Run `./scripts/prep.sh` (bash, in WSL). It:

1. Checks you have the .NET 11 SDK, the Pulumi CLI, and an Azure login.
2. Builds every project and runs the tests.
3. Creates and deploys the `dev` stacks for demos 1 and 2 (so the live `pulumi up` only replaces one file).
4. Creates the `friday` stack in demo 2.

Then by hand:

- [ ] **Thursday: one full dry run against real Azure.** Both demos, start to finish, with a timer. Nothing here has been deployed for real yet.
- [ ] Record a fallback video of each demo (StreamYard can share a second tab).
- [ ] Push the repo public, and check the QR code on slide 14 opens it.

## Day of

- [ ] VS Code: zoom to 150% or more (Ctrl+= or Cmd+=), terminal font 18 or more, a high-contrast theme, minimap off. Editor font Cascadia Mono, or turn ligatures off, so `=>` doesn't render as an arrow glyph.
- [ ] Close Slack, Teams, Outlook, and anything that pops toasts. Focus assist on.
- [ ] Browser tabs open: demo 1 site, demo 2 dev site, the repo.
- [ ] Terminal history cleared. One terminal per demo folder, already `cd`'d. `PULUMI_CONFIG_PASSPHRASE` set in each, if you need it.
- [ ] `git status` clean in every demo folder.
- [ ] `git pull`, then reload the VS Code window (Ctrl+Shift+P, "Developer: Reload Window") so IntelliSense uses the .NET 11 SDK.
- [ ] Every `pulumi preview`/`up` starts with ~10 lines of `dotnet build` output. Scroll past it to the diff.
- [ ] `az account show` returns the subscription you expect. No secrets on screen.
- [ ] Fallback videos open in a tab.

## Cleanup (after the talk)

Run `./scripts/cleanup.sh`. It destroys every stack the demos created, including protected resources and the single-file bonus stacks. Check the Azure portal for leftover resource groups that start with `hello-cloud`, `policy-`, `agent-`, or `single-file-`.
