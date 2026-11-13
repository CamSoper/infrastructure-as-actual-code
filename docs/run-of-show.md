# Run of show

**Infrastructure as (Actual) Code with C# 15 and .NET 11**
.NET Conf 2026 Community Days, Friday, November 13, 2:30 to 3:00 PM PST (4:30 CT). StreamYard, 1920 x 1080.
Check in with the host 15 minutes early. Be back 5 minutes before start.

Target: 25 minutes of content, 5 minutes of Q&A. Hard stop on content at 25:00.

| Time | Slide(s) | What happens |
|---|---|---|
| 0:00 | 1 to 5 | Cold open: define IaC in one line, then Terraform, one "if" per slide (ternary, `count = prod ? 1 : 0`, `dynamic`), then "config with extra steps" |
| 1:45 | 6 | Who's this guy (30 seconds, open source disclaimer) |
| 2:15 | 7 to 8 | What "actual code" means; Pulumi in one slide |
| 3:45 | 9 | **Demo 1: Hello, cloud** |
| 8:30 | 10 | C# 15 unions in one slide |
| 9:30 | 11 | **Demo 2: The compiler is your policy engine** |
| 14:30 | 12 to 13 | The hades story; the thesis |
| 16:30 | 14 | **Demo 3: Agent vs. compiler vs. the calendar** (the finale) |
| 23:30 | 15 | Take it home: repo, tests, single file |
| 24:30 | 16 | Thanks, Q&A |

If you're running long, cut in this order:

1. Demo 1, step 7 (the chat crew member). Add a name yourself. Chat lags 10 to 30 seconds, so if nobody's answered by the time you finish saying it, use "Rubber duck."
2. Slide 12 (the hades story). Go straight to slide 13.
3. Demo 2, step 11 (`pulumi preview --stack friday`). Point at `GoHomeException` instead. Demo 3 delivers that joke for real anyway.

**Never cut demo 3, steps 7 and 8.** They're the payoff.

---

## Demo 1: Hello, cloud (3:45 to 8:30)

**Folder:** `demos/01-hello-cloud` **Stack:** `dev`, already deployed

| # | Do | Say |
|---|---|---|
| 1 | Open `Pulumi.yaml`. | "The project file. A name, `runtime: dotnet`, and one setting: the Azure region. It's not the infrastructure; it tells Pulumi how to run my program." |
| 2 | Open `Program.cs`. Collapse both regions, then expand "The infrastructure". | "It's a console app. A `.csproj`, NuGet packages, `Program.cs`. That's the whole project." |
| 3 | Hover `SkuName.Standard_LRS`. | "IntelliSense for Azure. Every property, every allowed value, documented. LRS means locally redundant: one datacenter. Fine for a demo." |
| 4 | Point at `group.Name` in the storage account. | "That's a real reference. Pulumi works out the order from the code. No `dependsOn`." |
| 5 | Expand "The content". Open `Page.cs`. | "The website is a raw string literal. The go/no-go poll is LINQ." |
| 6 | Switch to the browser tab with the deployed site. | "This is live on Azure, right now." |
| 7 | "Chat, who else belongs on the launch crew?" Add one name to `crew` (`Program.cs`, line 35). | "Chat input, so it gets HTML-encoded. I've met chat." |
| 8 | `pulumi up` | "Preview first. It's diffing against what I deployed last time. One resource changes: the page gets replaced. Nothing else gets touched. If you've used Terraform, this is `terraform plan`." Select **yes**. |
| 9 | Refresh the browser. | "New crew member, GO for launch." |

**Land it:** "That's a loop, a string, and a list. In a real language. With a compiler watching."

**Fallback:** If Azure or Pulumi misbehaves, show the browser tab you opened before the talk and `git diff`. Say "the cloud is having a moment, here's the change," and move on.

---

## Demo 2: The compiler is your policy engine (9:30 to 14:30)

**Folder:** `demos/02-compiler-as-policy` **Stacks:** `dev` deployed; `friday` initialized, never deployed

Everything you need to type is already in the files, commented out. Use **Ctrl+K Ctrl+U** (Cmd+K Cmd+U on a Mac) to uncomment.

**What you're looking at**

- **The story:** environments are a closed list (a union). Every rule is a switch over that list. Add an environment and the compiler flags every rule that hasn't decided about it.
- **Stack vs. `Stage`:** a stack is one deployed copy of the program (same code, its own config and state). Pulumi gives you its name as a string. `Stage` is our type for what kind of environment that stack is, and `Site.Build` converts one to the other first thing.
- **`Stages.cs`:** `Dev` and `Staging` are empty marker records. `Prod` requires `OnCall`. `FromStack` is the one place a stack name (a string) becomes a `Stage` (a type). Its `_ =>` is fine, because strings are open-ended and the union isn't.
- **`Policy.cs`:** three switches, three rules. `Redundancy` picks the SKU, `IsProtected` sets delete protection, and `Banner` is what the site says. No `_ =>` anywhere, so the compiler checks that each one covers every `Stage`.
- **`.csproj`:** `WarningsAsErrors` with CS8509 ("switch doesn't handle every case") turns a warning into a failed build. That's the policy engine.
- **The 3 errors:** one per switch, at `Policy.cs` lines 9, 18, and 27.
- **The `friday` preview:** `Site.cs` works out the banner before creating anything, so `GoHomeException` stops the deploy before any resource exists.

| # | Do | Say |
|---|---|---|
| 1 | `Program.cs`. | "Same website as demo 1, split into files. Program.cs is one line: hand Pulumi the `Build` method." |
| 2 | F12 into `Site.Build`. Highlight line 12 (`StackName`), then line 14 (`Stages.FromStack`). Scroll past the resources; don't dwell. F12 into `FromStack`. | "Which stack am I deploying? A stack is one deployed copy of this program: same code, its own config, its own state. dev, prod, and soon friday. Pulumi hands me its name as a string, so first we turn it into a `Stage`: our own type for what kind of environment this stack is. Everything below asks the Stage, never the string." |
| 3 | `Stages.cs`: the records, the union, `FromStack`. | "Every environment is a type. And look: you can't make a `Prod` without naming who gets paged." |
| 4 | `Policy.cs`: three switches. | "Our deployment rules. Redundancy, delete protection, the banner. And notice what's missing: no `_ =>`. A discard arm is you telling the compiler to stop helping." |
| 5 | `CompilerAsPolicy.csproj`: the `WarningsAsErrors` line. | "CS8509 is the 'your switch missed a case' warning. This one line makes it an error. That's the difference between a policy and a suggestion." |
| 6 | Browser tab: the `dev` site's yellow banner. | Read it: "Dev: nothing here is real." |
| 7 | Say the setup: "Everybody knows the rule: never deploy on Friday afternoon. So let's make Friday afternoon an environment." In `Stages.cs`, uncomment the `FridayAfternoon` record (line 7) and the `, FridayAfternoon` line in the union (line 13). | |
| 8 | `dotnet build` | You'll see 6 error lines (each is printed twice); point at the "3 Error(s)" summary. Read one error aloud: "Policy.cs line 9: the switch doesn't handle FridayAfternoon." Then: "Three errors. One per rule. The compiler just did a code review of our entire deployment policy." Then the callback: "Remember the cold open? In Terraform this was a ternary on a string and a count of one-or-zero. Add an environment and nothing tells you. Here, it's three build errors." |
| 9 | In `Policy.cs`, uncomment the three `FridayAfternoon =>` arms (lines 14, 23, 32) and `GoHomeException` at the bottom (line 53). In `Stages.cs`, uncomment the `"friday"` line (line 24). Read the comments out loud. | "Maximum redundancy, maximum anxiety." |
| 10 | `dotnet build` | "Green." |
| 11 | Clear the terminal (Ctrl+L). `pulumi preview --stack friday` | Scroll to the message and read it: "It's Friday afternoon. The deploy can wait. Close the laptop." Then, slowly: "Which is a joke. Probably." |

**Land it:** "The compiler didn't write the rule. It made me decide, in every single place. And it'll do the same for whoever touches this next, human or not."

**Fallback:** All of this is compile time. If `pulumi preview` is slow or broken, skip step 11 and just point at the `GoHomeException` line.

**After the talk:** `git checkout -- .` in this folder to put the comments back.

---

## Demo 3: Agent vs. compiler vs. the calendar (16:30 to 23:30)

**Folder:** `demos/03-agent-vs-compiler` (demo 2's end state) **Stack:** `pr-1013`, pre-deployed by `prep.sh`

**Don't mention `FridayRule.cs`.** Close the VS Code sidebar (Ctrl+B / Cmd+B) before step 1 so it isn't sitting in the file list. Step 7 is the surprise.

| # | Do | Say |
|---|---|---|
| 1 | Open `AGENTS.md`. Read the last two lines off the screen. | "Do NOT deploy on Friday afternoons." Then the next line: "Bold is a suggestion." |
| 2 | In `Stages.cs`, uncomment the `Preview` record (line 8) and the `, Preview` line (line 15). `dotnet build`. | "Per-pull-request preview environments. I added the type. And now the build is broken: three errors. Same as last time. Except this time I'm not fixing them." |
| 3 | Start your agent in this folder. Paste `prompts/preview-environments.md`. **Start a 90-second timer.** | "Here's the prompt. 'I added a Preview environment and the build is broken. Fix it.' Plus one line on how preview stacks are named. The compiler wrote the rest of the to-do list." |
| 4 | Narrate while it works. If it hits 90 seconds, cut it off and use the fallback. | "Watch where it goes. Not a search. Straight to the errors." If the agent mentions `FridayRule` or Friday, lean in: "Even the robot's warning me." |
| 5 | `git diff` | Check out loud: "Every switch handled? No sneaky `_ =>`? Preview is cheap, unprotected, and the banner says which PR? And `FromStack` turns `pr-1013` into a Preview?" **If that last one is missing, use the fallback.** Without it, step 7 fails with "isn't an environment we know how to build" instead of the punchline. Read the banner line: "Preview of pull request number 1013." |
| 6 | `dotnet build` yourself. | "Trust, but compile." |
| 7 | Clear the terminal. `pulumi up --stack pr-1013` | It fails with `GoHomeException`. Select-highlight the `GoHomeException:` line so it doesn't read as a crash, and let it sit. Then: "...Told you. This Friday." Read the end of the message: "Or sign for it." Open `FridayRule.cs` for five seconds: "The bold rule was a suggestion. This one's code." |
| 8 | `pulumi up --stack pr-1013 --config blame=Cam`, select **yes**, open the URL. | "Fine. I'll sign for it." On the page, read the footer: "Shipped on a Friday afternoon. Blame: Cam." (No footer? Ctrl+F5, the browser cached the prep version.) |

**Land it (slowly):** "Written by a robot. Reviewed by a compiler. Deployed by a human, who had to sign for it. On a Friday."

**Fallback:** If the agent is slow (90 seconds), wrong, or offline, cancel it and run:

```
git checkout -- . ; git apply --ignore-whitespace fallback/preview-environment.patch
```

That resets the uncomment from step 2 and applies the reference solution (swap in your agent's rehearsal output if you like it better). Say "here's what it did in rehearsal" and carry on from step 5.

**If `pulumi up` doesn't throw in step 7** (clock or time zone surprise): say "Huh. The rule says Friday afternoon, Pacific. Somebody check my clock." Select **yes** at that prompt, skip step 8, and open the URL. The footer only appears on a Friday afternoon, so read the banner instead.

**Hard stop at 23:30.** If `pulumi up` is still going, move to slide 15 and come back to the URL at the end.

**After the talk:** `git checkout -- .` and delete any new files. `cleanup.sh` handles the stack.

---

## Prep (Thursday, and again Friday morning)

**WSL setup (once).** Everything runs inside WSL: the .NET 11 SDK, the Pulumi CLI, the Azure CLI, and your agent all need installing there, not just on Windows. Clone the repo into the Linux filesystem (`~/src/...`), not `/mnt/c/...`, which is slow and invites CRLF trouble. Open it with `code .` from WSL so VS Code uses the WSL remote, and its terminal is bash. If `az login` in WSL can't open a browser, use `az login --use-device-code`.

Using a local Pulumi backend (`pulumi login --local`)? Set `PULUMI_CONFIG_PASSPHRASE` in the shell first, or every command stops to ask for it. On stream, that's a password prompt on screen.

Run `./scripts/prep.sh` (bash, in WSL). It:

1. Checks you have the .NET 11 SDK, the Pulumi CLI, and an Azure login.
2. Builds every project and runs the tests.
3. Creates and deploys the `dev` stacks for demos 1 and 2 (so the live `pulumi up` only replaces one file).
4. Creates the `friday` stack in demo 2.
5. Creates the `pr-1013` stack in demo 3, deploys it once with the reference solution, then resets the folder with `git checkout -- .`. The live `pulumi up` then replaces one file instead of creating a storage account.

Then by hand:

- [ ] **Thursday: one full dry run against real Azure.** All three demos, start to finish, with a timer. Nothing here has been deployed for real yet.
- [ ] Rehearse demo 3 with your agent at least twice. If its output differs from `fallback/preview-environment.patch` in an interesting way, keep the better one.
- [ ] Agent permissions: allow `dotnet build` without asking, so you don't approve prompts on stream. **Deny** `pulumi` outright. An agent that runs `pulumi preview` on Friday afternoon spoils step 7, and one that sets `blame` to get past it kills the joke for good.
- [ ] Rehearse the Friday payoff on Thursday: it won't fire. To see it, temporarily point `FridayRule.Clock` at a Friday, or just trust the tests (`Friday_afternoon_blocks_every_stack`).
- [ ] Record a fallback video of each demo (StreamYard can share a second tab).
- [ ] Push the repo public, and check the QR code on slide 15 opens it.

## Day of

- [ ] VS Code: zoom to 150% or more (Ctrl+= or Cmd+=), terminal font 18 or more, a high-contrast theme, minimap off. Editor font Cascadia Mono, or turn ligatures off, so `=>` doesn't render as an arrow glyph.
- [ ] Close Slack, Teams, Outlook, and anything that pops toasts. Focus assist on.
- [ ] Browser tabs open: demo 1 site, demo 2 dev site, the repo.
- [ ] Demo 3 window: sidebar closed (Ctrl+B), so `FridayRule.cs` isn't on screen.
- [ ] Terminal history cleared. One terminal per demo folder, already `cd`'d. `PULUMI_CONFIG_PASSPHRASE` set in each, if you need it.
- [ ] `git status` clean in every demo folder.
- [ ] `git pull`, then reload the VS Code window (Ctrl+Shift+P, "Developer: Reload Window") so IntelliSense uses the .NET 11 SDK.
- [ ] Every `pulumi preview`/`up` starts with ~10 lines of `dotnet build` output. Scroll past it to the diff.
- [ ] `az account show` returns the subscription you expect. No secrets on screen.
- [ ] Agent signed in and warm (run one throwaway prompt).
- [ ] A backup crew name for demo 1, in case chat is slow.
- [ ] Fallback videos open in a tab.

## Cleanup (after the talk)

Run `./scripts/cleanup.sh`. It destroys every stack the demos created, including protected resources and the single-file bonus stacks. Check the Azure portal for leftover resource groups that start with `hello-cloud`, `policy-`, `agent-`, or `single-file-`.
