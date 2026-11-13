# Speaker notes

Infrastructure as (Actual) Code with C# 15 and .NET 11. Cam Soper, .NET Conf 2026 Community Days.

## Slide 1: Infrastructure as (Actual) Code with C# 15 and .NET 11

[Host intro. Smile. Don't start talking about yourself yet; the cold open does the work.]

Thanks! Before I say who I am, I want to show you something.

## Slide 2: Infrastructure as code, they said

⏱️ RESET THE TIMER NOW ⏱️

COLD OPEN (0:00 to 1:45). Keep it moving: about 25 seconds a slide.

"Infrastructure as code: you describe your cloud in files, and a tool makes reality match."

"Real Terraform. One storage account, named per environment."

[Click: the replication line.]
"Prod gets zone-redundant. A ternary on a string. Fine."

## Slide 3: Infrastructure as code, they said

[Click 1: the lock.]
"Prod needs a delete lock."

[Click 2: count.]
"HCL has no if statement, so you ask for one-or-zero of it. [Beat.] That's an if statement."

## Slide 4: Infrastructure as code, they said

[Click 1: the block.]
"Prod needs versioning. It's a nested block, so count doesn't work."

[Click 2: for_each.]
"So you loop over a list of one thing, or nothing. [Beat.] That's ALSO an if statement."

"And var.env is a string. Misspell prod, plan is green, and production quietly gets the dev setup. You find out during the outage. Naturally, on a Friday afternoon. Unless you're Microsoft, and then Thursday afternoons are an option."

[Slowly.] "Probably this Friday."

## Slide 5: That's not code. That's config with extra steps.

"That's not code. That's config with extra steps. The C in HCL literally stands for Configuration."

"Terraform's a great tool. But today: infrastructure as actual code. In C#. With a compiler that says no."

## Slide 6: Cam Soper

WHO'S THIS GUY (1:45 to 2:15, keep it to 30 seconds)

I'm Cam. I spent 13 years at Microsoft, most of it on .NET and Azure, and some of you might remember me from On .NET Live. These days I'm in systems engineering at Pulumi. I built and run our automated docs QA system, and now I'm working on Pulumi's agentic SDLC.

Which means I put AI agents into production for a living. Hold that thought; it matters in demo 3.

Quick disclaimer, since I work there: everything you'll see today is open source. No sign-up, no paid features. The demos are the pitch; I'm not going to do a sales slide.

And this isn't a conference toy: my home lab and my domains are deployed with exactly this.

## Slide 7: What “actual code” gets you

WHAT "(ACTUAL)" MEANS (2:15 to 3:00)

Four clicks: each card fades in as you name it.

When I say "actual code," I mean four things your app code already gets and your config files don't.

[Click 1: A compiler.] A compiler. It reads every line before anything deploys.

[Click 2: Types and IntelliSense.] Types and IntelliSense: every property and every allowed value, and you can F12 into a storage account.

[Click 3: Tests.] Tests: you can unit-test infrastructure without touching a cloud.

[Click 4: Packages.] And packages: infrastructure you can share on NuGet.

Every demo today ticks at least one of these.

## Slide 8: Pulumi, in one slide

PULUMI IN ONE SLIDE (3:00 to 3:45). Four clicks: the same three as the Terraform slides, then the language list.

Remember that Terraform? Here's the same thing in C#, with Pulumi.

The storage account is just "new StorageAccount." It doesn't create anything right then; it declares that you want one. When you run "pulumi up," Pulumi runs your program, diffs what you declared against your last deployment, shows you the plan, and asks before it changes anything. If you've used Terraform, the preview is "terraform plan."

[Click 1.] The SKU: a ternary. Same as before. C# has those too.

[Click 2.] The lock only exists in prod. So... it's an if statement. An actual if statement. Not one-or-zero of something.

[Click 3.] Versioning only in prod? It goes in the same if. Not a loop over a list of one thing. Just code.

[Beat.] Yes, it's still a string comparison on "prod." Hold that thought; that's what C# 15 fixes in a few minutes.

[Click 4: the language row.] And C# is my pick, not the only one. Same engine in TypeScript, JavaScript, Python, Go, the rest of .NET, Java, and YAML. [Beat.] And yes, HCL. Pulumi runs HCL now. So if your team has a pile of Terraform, you don't have to rewrite it to start; the new stuff can be actual code.

Open source, and no account needed: "pulumi login --local" keeps your state in a folder on your machine, or point it at an Azure blob container.

Enough slides. Let's deploy something.

## Slide 9: Demo: Hello, cloud

DEMO 1: HELLO, CLOUD (3:45 to 8:30). Full cue card in docs/run-of-show.md.

Folder: demos/01-hello-cloud. Stack "dev" is ALREADY deployed.

1. Pulumi.yaml. "The project file. A name, runtime: dotnet, and one setting, the Azure region. It's not the infrastructure; it tells Pulumi how to run my program."
2. Program.cs. Collapse both regions, expand "The infrastructure".
   "It's a console app. Program.cs, a csproj, NuGet packages. That's it."
3. Hover SkuName.Standard_LRS: "IntelliSense for Azure. LRS is locally redundant: one datacenter."
4. Point at group.Name: "A real reference. No dependsOn."
5. Expand "The content", open Page.cs: "A raw string literal. The poll is LINQ."
6. Browser tab: "This is live on Azure, right now."
7. "Who else belongs on the launch crew? The rubber duck. Every debugger's first crew member." Add "Rubber duck" to crew in Program.cs (new line after "Unit tests", line 41). "It's user input, so it gets HTML-encoded."
8. pulumi up. "It's diffing against my last deployment. One resource changes: the page gets replaced. This is terraform plan." Say yes.
9. Refresh the browser. New crew member is GO.

Line to land: "That's a loop, a string, and a list. In a real language. With a compiler watching."
Fallback: browser tab of the already-deployed site + git diff of the change.

## Slide 10: C# 15: union types

UNIONS IN ONE SLIDE (8:30 to 9:30). Three clicks, one per bullet.

[Click 1: "Exactly one of a fixed set."] New in C# 15: union types. A Stage is a Dev, or a Staging, or a Prod. Exactly one. Nothing else.

Each case is its own type. Dev and Staging are records with nothing inside: they don't carry any data, because being Dev is the whole message. Prod carries an OnCall name. You literally cannot construct production without naming who gets paged. Invalid states, unrepresentable.

[Click 2: "Miss a case: compiler error."] The switch handles every case. Leave one out and you get CS8509: "this switch doesn't handle every case." It's only a warning by default. One line in the csproj makes it an error. That's the whole policy engine.

[Click 3: "No default arm needed."] And look what's missing: no underscore default arm. It doesn't need one, because the compiler knows every possible Stage. Which means if I add a fourth one...

(If inheritance comes up in Q&A: C# 15 also adds "closed" class hierarchies that get the same exhaustiveness checks.)

## Slide 11: Demo: The compiler is your policy engine

DEMO 2: THE COMPILER IS YOUR POLICY ENGINE (9:30 to 14:30)
Folder: demos/02-compiler-as-policy. "dev" is deployed. "friday" is initialized, never deployed.

[Open Program.cs.]
"Same website as demo 1, split into files. Program.cs is one line: hand Pulumi the Build method."

[F12 into Site.Build. Highlight line 12, StackName.]
"First thing it asks: which stack am I deploying?"
"A stack is one deployed copy of this program. Same code, its own config, its own state. dev, prod, and in a minute, friday. Pulumi hands me the stack's name, and that's a string."

[Highlight line 14, Stages.FromStack. Scroll past the resources; don't dwell.]
"So the first thing we do is turn that string into a Stage: our own type for what kind of environment this stack is. Everything below asks the Stage, never the string." 

[F12 into FromStack. Stages.cs.]
"Here's the one place a stack name turns into a type." (If anyone asks about its "_ =>": strings are open-ended. The union isn't.)

[Scroll up. Highlight the three records.]
"Every environment is a type. Dev and Staging carry nothing; being Dev is the whole message. Prod needs an OnCall name. You can't make a Prod without naming who gets paged."

[Highlight the union.]
"And Stage is exactly one of those three. A closed list."

[Open Policy.cs. Scroll the three switches.]
"Our deployment rules. Which SKU, whether it's delete-protected, what the banner says. Each one's a switch over Stage. Notice what's missing: no underscore default arm. A discard arm is you telling the compiler to stop helping."

[Open the csproj. Highlight WarningsAsErrors.]
"CS8509 means a switch missed a case. It's a warning by default. This one line makes it an error. That's the whole policy engine."

[Browser: the dev site. Point at the yellow banner.]
"Dev: nothing here is real. That's the Dev arm of the banner switch, live."

[Back to Stages.cs.]
"Everybody knows the rule: never deploy on Friday afternoon. So let's make Friday afternoon an environment."
[Uncomment line 7 (the record) and line 13 (", FridayAfternoon").]

[Terminal: dotnet build. 3 errors, each printed twice. Highlight one.]
"Policy.cs, line 9: this switch doesn't handle FridayAfternoon. Three errors. One per rule. The compiler just reviewed our entire deployment policy."
"Remember the cold open? In Terraform, add an environment and nothing tells you. Here, three build errors."

[Policy.cs: uncomment lines 14, 23, 32, and GoHomeException at line 53. Stages.cs: uncomment line 24, the "friday" line.]
Read the comments: "Maximum redundancy, maximum anxiety. Nobody touches anything."
"It made me decide. Every rule, for the new environment."

[dotnet build.]
"Green."

[Clear the terminal. pulumi preview --stack friday. Highlight the message.]
"It's Friday afternoon. The deploy can wait. Close the laptop."
[Slowly.] "Which is a joke. Probably."
(It fails before any resource exists: the banner is worked out first.)

Land it: "The compiler didn't write the rule. It made me decide. Every single place."
Fallback: if Pulumi is sad, skip the preview and show the GoHomeException line.

## Slide 12: A rule for my agents. In bold.

THE REAL STORY (14:30 to 16:00). Cut this slide first if you're long. Three clicks: source, quote, confession.

[Click 1: "From my own CLAUDE.md."] This is a real line from the instructions file in my own infrastructure repo, the one that runs my domains and home-lab services.

[Click 2: the quote. Read it out loud.] Cloudflare Access is a login screen that sits in front of a website. My server called hades already does its own OAuth sign-in. Put Access in front of it and you log in twice.

Confession: I didn't write that rule. Claude did. [Click 3: "Written by Claude. Checked in by me. Unread."] I didn't read it, and I checked it in. So now it's an AI writing rules for other AIs, in bold, with "NOT" in capitals, and the only enforcement mechanism is vibes. Because the types I had couldn't say it.

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
2. Stages.cs: uncomment the Preview record (line 8) and ", Preview" (line 15). dotnet build: 3 errors. "This time I'm not fixing them."
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
Hard stop at 23:30. If pulumi up is still going, go to slide 15 and come back to the URL at the end.

## Slide 15: Take it home

TAKE IT HOME (23:30 to 24:30)

Everything's in the repo: https://github.com/CamSoper/infrastructure-as-actual-code

All three demos, the slides, and these speaker notes. There's a test project that runs this whole Azure stack against a fake cloud, so you can try the union stuff with no Azure account at all.

And as a bonus: the whole thing in a single C# file. dotnet run deploy.cs. One gotcha that'll bite you: file-based apps set PublishAot=true by default, which turns off reflection-based JSON, and Pulumi needs it. Add "#:property PublishAot=false" at the top. Ask me how I know.

[The QR code and link are on screen. Leave this slide up for a few seconds.]

## Slide 16: Thank you

Q&A (24:30 to 30:00)

Seed questions, in case Q&A is quiet:

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
