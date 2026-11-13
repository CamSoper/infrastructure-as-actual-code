# Speaker notes

Infrastructure as (Actual) Code with C# 15 and .NET 11. Cam Soper, .NET Conf 2026 Community Days.

## Slide 1: Infrastructure as (Actual) Code with C# 15 and .NET 11

[Host intro. Smile. Don't start talking about yourself yet; the cold open does the work.]

Thanks! Before I say who I am, I want to show you something.

## Slide 2: Infrastructure as code, they said

COLD OPEN (0:00 to 1:30). Three slides, one new "if" per slide.

Show of hands... well, it's a livestream, so: drop a 🙋 in chat if you've ever written "count equals something question-mark one colon zero."

[Beat. Read a couple of chat replies. There will be replies.]

This is real Terraform. One storage account for a website. A variable says which environment we're in, and the dollar-curly bits are string interpolation: site-dev, site-prod.

[Click: the replication line.] Prod should be zone-redundant, everything else can be cheap. So: a ternary on a string. Fine. Totally reasonable.

## Slide 3: Infrastructure as code, they said

[The storage account is folded up. Same thing as before.]

[Click 1: the lock.] Now prod needs a delete lock, so nobody fat-fingers the production site away.

[Click 2: count.] But only in prod. HCL doesn't have an if statement, so you ask for one-or-zero of it. [Beat.] That's an if statement.

And now that lock is a list. Anything that refers to it says keep-square-bracket-zero, or wraps it in try(). Forever.

## Slide 4: Infrastructure as code, they said

[Click 1: the block.] Last one. Prod needs blob versioning, so you can undo a bad deploy of the site. That's a nested block inside the storage account, so count doesn't work.

[Click 2: for_each.] Instead you loop over a list that has one thing in it, or nothing. [Beat.] That's ALSO an if statement.

Now look at what's NOT on any of these slides. No if. No switch. No loop that does anything except build a list. Every decision is squeezed into one expression that produces one value. That's configuration.

And what type is var.env? A string. Misspell "prod" in your tfvars, and plan is green: production quietly gets the dev SKU, no lock, no versioning. You find out during the outage. On a Friday.

[Slowly.] Probably this Friday.

Add a staging environment? Nothing tells you staging needs a decision about any of this. It just falls through to "not prod."

We call this "infrastructure as code."

[If chat says "just add a validation block" or "use a map": they're right, and say so. "Yep. Do that, and you get an error at plan time, for the one environment you planned. Hold that thought: in demo 2 the compiler checks every rule, for every environment, before anything runs."]

## Slide 5: That's not code. That's config with extra steps.

That's not code. That's config with extra steps.

And don't take my word for it: the C in HCL stands for Configuration. They named it. I'm just reading the label.

To be fair: Terraform is a great tool, and HCL is a real language with real tooling. I'm not here to dunk on it. But environment logic as string comparisons and one-or-zero counts is configuration that learned a few tricks. It's not code with a compiler that knows every case.

[Optional aside, if chat's having fun:] HCL even has template directives: percent-curly-if. Classic ASP called. It wants its delimiters back.

Today: infrastructure as actual code. In C#. With a compiler that says no.

## Slide 6: Cam Soper

WHO'S THIS GUY (1:30 to 2:00, keep it to 30 seconds)

I'm Cam. I spent 13 years at Microsoft, most of it on .NET and Azure, and some of you might remember me from On .NET Live. These days I'm in systems engineering at Pulumi. I built and run our automated docs QA system, and now I'm working on Pulumi's agentic SDLC.

Which means I put AI agents into production for a living. Hold that thought; it matters in demo 3.

Quick disclaimer, since I work there: everything you'll see today is open source. No sign-up, no paid features. The demos are the pitch; I'm not going to do a sales slide.

And this isn't a conference toy: my home lab and my domains are deployed with exactly this.

## Slide 7: What “actual code” gets you

WHAT "(ACTUAL)" MEANS (2:00 to 2:45)

[Four clicks: each card fades in as you name it.]

When I say "actual code," I mean four things your app code already gets and your YAML doesn't.

A compiler. Types and IntelliSense: you can F12 into a storage account. Tests: you can unit-test infrastructure without touching a cloud. And packages: infrastructure you can share on NuGet.

Every demo today ticks at least one of these.

## Slide 8: Pulumi, in one slide

PULUMI IN ONE SLIDE (2:45 to 3:30). Three clicks, same order as the Terraform slide.

Remember that Terraform? Here's the same thing in C#, with Pulumi.

The storage account is just "new StorageAccount." It doesn't create anything right then; it declares that you want one. When you run "pulumi up," Pulumi runs your program, diffs what you declared against your last deployment, shows you the plan, and asks before it changes anything. If you've used Terraform, the preview is "terraform plan."

[Click 1.] The SKU: a ternary. Same as before. C# has those too.

[Click 2.] The lock only exists in prod. So... it's an if statement. An actual if statement. Not one-or-zero of something.

[Click 3.] Versioning only in prod? It goes in the same if. Not a loop over a list of one thing. Just code.

[Beat.] Yes, it's still a string comparison on "prod." Hold that thought; that's what C# 15 fixes in a few minutes.

Open source, and no account needed: "pulumi login --local" keeps your state in a folder on your machine, or point it at an Azure blob container.

Enough slides. Let's deploy something.

## Slide 9: Demo: Hello, cloud

DEMO 1: HELLO, CLOUD (3:30 to 8:30). Full cue card in docs/run-of-show.md.

Folder: demos/01-hello-cloud. Stack "dev" is ALREADY deployed.

1. Program.cs. Collapse both regions, expand "The infrastructure".
   "It's a console app. Program.cs, a csproj, NuGet packages. That's it."
2. Hover SkuName.Standard_LRS: "IntelliSense for Azure. LRS is locally redundant: one datacenter."
3. Point at group.Name: "A real reference. No dependsOn."
4. Expand "The content", open Page.cs: "A raw string literal. The poll is LINQ."
5. Read YAML's row out loud: "YAML: no-go. Unexpected indentation, line 7." Pause.
6. Browser tab: "This is live on Azure, right now."
7. "Chat: who else belongs on the launch crew?" Add one name to crew. (Backup if chat lags: "Rubber duck".)
8. pulumi up. "It's diffing against my last deployment. One resource changes: the page gets replaced. This is terraform plan." Say yes.
9. Refresh the browser. New crew member is GO.

Line to land: "That's a loop, a string, and a list. In a real language. With a compiler watching."
Fallback: browser tab of the already-deployed site + git diff of the change.

## Slide 10: C# 15: union types

UNIONS IN ONE SLIDE (8:30 to 9:30). Three clicks: one per bullet.

New in C# 15: union types. A Stage is a Dev, or a Staging, or a Prod. Exactly one. Nothing else.

Two details I love. First: Prod carries an OnCall name. You literally cannot construct production without naming who gets paged. Invalid states, unrepresentable.

Second: that switch has no default arm. It doesn't need one, because the compiler knows every possible Stage. Which means if I add a fourth one...

And warning CS8509, "this switch doesn't handle every case," is only a warning by default. One line in the csproj makes it an error. That's the whole policy engine.

(If someone in chat asks about inheritance: C# 15 also adds "closed" class hierarchies that get the same exhaustiveness checks.)

## Slide 11: Demo: The compiler is your policy engine

DEMO 2: THE COMPILER IS YOUR POLICY ENGINE (9:30 to 14:30). Full cue card in docs/run-of-show.md.

Folder: demos/02-compiler-as-policy. Stack "dev" ALREADY deployed; stack "friday" initialized, never deployed.

1. Stages.cs: the records, the union, FromStack. "You can't make a Prod without naming who gets paged."
2. Policy.cs: three switches, no "_ =>". "A discard arm is you telling the compiler to stop helping."
3. The csproj: "CS8509 is the missed-case warning. This line makes it an error."
4. Dev site (browser tab). Read the banner: "Dev: nothing here is real."
5. "You know what environment we don't have? The one this talk is in." Uncomment FridayAfternoon in Stages.cs (2 lines).
6. dotnet build: 3 errors. Read one out loud: "Policy.cs line 9: doesn't handle FridayAfternoon." "One per rule. The compiler just reviewed our entire deployment policy."
   Then, the callback: "Remember the cold open? In Terraform this was a ternary on a string and a count of one-or-zero. Add an environment and nothing tells you. Here, it's three build errors."
7. Uncomment the FridayAfternoon arms in Policy.cs, the "friday" line in Stages.cs, and GoHomeException.
   Read the comments out loud: "maximum redundancy, maximum anxiety."
8. dotnet build: green.
9. Clear the terminal. pulumi preview --stack friday. Read it: "It's Friday afternoon. The deploy can wait. Close the laptop." Then, slowly: "Which is a joke. Probably."

Line to land: "The compiler didn't write the rule. It made me decide. Every single place."
Fallback: it's all compile time. If Pulumi is sad, skip step 9 and show the GoHomeException line.

## Slide 12: A rule for my agents. In bold.

THE REAL STORY (14:30 to 16:00). Cut this slide first if you're long. Three clicks: source, quote, confession.

This is a real line from the instructions file in my own infrastructure repo, the one that runs my domains and home-lab services. Cloudflare Access is a login screen that sits in front of a website. My server called hades already does its own OAuth sign-in. Put Access in front of it and you log in twice.

Confession: I didn't write that rule. Claude did. I didn't read it, and I checked it in. [Click: "Written by Claude. Checked in by me. Unread."] So now it's an AI writing rules for other AIs, in bold, with "NOT" in capitals, and the only enforcement mechanism is vibes. Because the types I had couldn't say it.

The union version: every service gets exactly one auth posture. hades is SelfOAuth, full stop. "Access and its own OAuth" isn't a rule you can forget. It's a sentence you can't write. And if I ever add a fourth posture, the compiler finds every place that has to care.

[Optional: flash the real CLAUDE.md. Crop out the account and zone IDs at the top first.]

## Slide 13: A rule in an agent's instructions is a suggestion. A rule in the type system is enforced.

THE THESIS (16:00 to 16:30). Pause on this one. Let it sit.

A rule written in an agent's instructions is a suggestion. A rule written in the type system is enforced, no matter who, or what, wrote the change.

I put agents into production for a living. That's exactly why I trust them as far as the compiler does, and no further.

So let's test that. Let's hand the keyboard to a robot.

## Slide 14: Demo: Agent vs. compiler

DEMO 3: AGENT VS. COMPILER (16:30 to 23:30). The finale. Full cue card in docs/run-of-show.md.

Folder: demos/03-agent-vs-compiler (demo 2's end state). Stack "pr-1013" pre-deployed by prep.sh.
Don't mention FridayRule.cs until step 7.

0. Close the sidebar (Ctrl+B) so FridayRule.cs isn't on screen.
1. AGENTS.md. Read the last two lines off the screen: "Do NOT deploy on Friday afternoons." "Bold is a suggestion."
2. Stages.cs: uncomment the Preview record and ", Preview". dotnet build: 3 errors. "This time I'm not fixing them."
3. Start the agent, paste prompts/preview-environments.md. START A 90-SECOND TIMER.
   "I added a Preview environment and the build is broken. Fix it." Plus how preview stacks are named. "The compiler wrote the rest of the to-do list."
4. Narrate: "Watch where it goes. Not a search. Straight to the errors." Cut it off at 90 seconds.
   If it mentions FridayRule or Friday: "Even the robot's warning me."
5. git diff. Every switch handled? No "_ =>"? Cheap, unprotected? Read the banner: "Preview of pull request number 1013."
   FromStack maps "pr-1013" to Preview? If NOT, use the fallback, or step 7 fails with the wrong error.
6. dotnet build, yourself. "Trust, but compile."
7. Clear the terminal. pulumi up --stack pr-1013. It throws GoHomeException, because it's actually Friday afternoon.
   Highlight the GoHomeException line so it doesn't read as a crash. Let it sit. "...Told you. This Friday."
   Read the end: "Or sign for it." Flash FridayRule.cs: "The bold rule was a suggestion. This one's code."
8. pulumi up --stack pr-1013 --config blame=Cam. Yes. Open the URL (Ctrl+F5 if no footer). Read the footer: "Shipped on a Friday afternoon. Blame: Cam."

Land it, slowly: "Written by a robot. Reviewed by a compiler. Deployed by a human, who had to sign for it. On a Friday."

Fallback (agent slow, wrong, or offline):
  git checkout -- . ; git apply --ignore-whitespace fallback/preview-environment.patch
Then steps 5 to 8. If step 7 doesn't throw: "Huh. The rule says Friday afternoon, Pacific. Somebody check my clock." Say yes, skip step 8, open the URL, read the banner.
Hard stop at 23:30.

## Slide 15: Take it home

TAKE IT HOME (23:30 to 24:30)

Everything's in the repo: https://github.com/CamSoper/infrastructure-as-actual-code

All three demos, the slides, and these speaker notes. There's a test project that runs this whole Azure stack against a fake cloud, so you can try the union stuff with no Azure account at all.

And as a bonus: the whole thing in a single C# file. dotnet run deploy.cs. One gotcha that'll bite you: file-based apps set PublishAot=true by default, which turns off reflection-based JSON, and Pulumi needs it. Add "#:property PublishAot=false" at the top. Ask me how I know.

[Drop the repo link in chat now.]

## Slide 16: Thank you

Q&A (24:30 to 30:00)

Seed questions, in case chat is quiet:

- "Can I use this with the Bicep/ARM I already have?"
  Yes. "pulumi import" adopts existing resources into code, and there are converters for ARM and Terraform. [Verify the Bicep converter's status before you promise it.]
- "Why not just use Aspire?"
  Aspire describes your app's topology and wires up local dev. This is general infrastructure as typed code. They solve different problems; don't claim an integration.
- "Do I need Pulumi Cloud?"
  No. pulumi login --local, or point state at an Azure blob container.
- "Does the Pulumi CLI run file-based apps directly?"
  Not that I've tested. That's why the bonus uses the Automation API, which works today.
- "Bicep has types too. Why switch?"
  It does, and it's good. The difference is exhaustiveness: add an environment, and C# finds every rule that has to handle it. And you get NuGet, unit tests, and the same language as your app.
- "Can agents run pulumi up?"
  They can. The repo's AGENTS.md says they don't. That's a suggestion. Use credentials that can only preview, and make the human the one with the deploy button.
