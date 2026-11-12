// The website, now with an environment banner.

using System.Net;

static class Page
{
    public static string Render(Banner banner, IEnumerable<string> crew, string? footer = null)
    {
        var poll = crew
            .Select(member => (member, status: "GO"))
            .Append((member: "YAML", status: "NO-GO (unexpected indentation, line 7)"));

        // HtmlEncode, because anything a human (or a chat) typed is untrusted input.
        var rows = string.Join("\n", poll.Select(p =>
            $"""      <li><span class="who">{WebUtility.HtmlEncode(p.member)}</span> <span class="{(p.status == "GO" ? "go" : "nogo")}">{p.status}</span></li>"""));

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Mission control: {{WebUtility.HtmlEncode(banner.Text)}}</title>
              <style>
                body { font-family: system-ui, sans-serif; background: #120838; color: #ffffff; margin: 0; padding: 2rem; }
                main { max-width: 40rem; margin: 0 auto; }
                .banner { background: {{banner.Color}}; color: #120838; font-weight: 700; font-size: 1.25rem; padding: 0.75rem 1rem; border-radius: 0.5rem; }
                h1 { font-size: 2.5rem; margin-bottom: 0.25rem; }
                ul { list-style: none; padding: 0; font-size: 1.25rem; }
                li { display: flex; justify-content: space-between; padding: 0.6rem 0; border-bottom: 1px solid #2e1a9e; }
                .go { color: #28c2d1; font-weight: 700; }
                .nogo { color: #f7b548; font-weight: 700; }
                footer { margin-top: 2rem; color: #f7b548; font-weight: 700; }
              </style>
            </head>
            <body>
              <main>
                <p class="banner" role="status">{{WebUtility.HtmlEncode(banner.Text)}}</p>
                <h1>🚀 Go/no-go poll</h1>
                <ul>
            {{rows}}
                </ul>
                {{(footer is null ? "" : $"<footer>{WebUtility.HtmlEncode(footer)}</footer>")}}
              </main>
            </body>
            </html>
            """;
    }
}
