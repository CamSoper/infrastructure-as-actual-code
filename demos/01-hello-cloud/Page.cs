// The website. Yes, it's HTML in a C# raw string literal. No, I'm not sorry.

using System.Net;

static class Page
{
    public static string Render(IEnumerable<string> crew)
    {
        // Everyone is GO. YAML gets polled too, because we're fair.
        var poll = crew
            .Select(member => (member, status: "GO"))
            .Append((member: "YAML", status: "NO-GO (unexpected indentation, line 7)"));

        // HtmlEncode, because names suggested by a live YouTube chat are untrusted input.
        var rows = string.Join("\n", poll.Select(p =>
            $"""      <li><span class="who">{WebUtility.HtmlEncode(p.member)}</span> <span class="{(p.status == "GO" ? "go" : "nogo")}">{p.status}</span></li>"""));

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Mission control: Hello, cloud</title>
              <style>
                body { font-family: system-ui, sans-serif; background: #120838; color: #ffffff; margin: 0; padding: 2rem; }
                main { max-width: 40rem; margin: 0 auto; }
                h1 { font-size: 2.5rem; margin-bottom: 0.25rem; }
                p.sub { color: #b9aaee; margin-top: 0; }
                ul { list-style: none; padding: 0; font-size: 1.25rem; }
                li { display: flex; justify-content: space-between; padding: 0.6rem 0; border-bottom: 1px solid #2e1a9e; }
                .go { color: #28c2d1; font-weight: 700; }
                .nogo { color: #f7b548; font-weight: 700; }
              </style>
            </head>
            <body>
              <main>
                <h1>🚀 Go/no-go poll</h1>
                <p class="sub">Deployed from C# with Pulumi.</p>
                <ul>
            {{rows}}
                </ul>
              </main>
            </body>
            </html>
            """;
    }
}
