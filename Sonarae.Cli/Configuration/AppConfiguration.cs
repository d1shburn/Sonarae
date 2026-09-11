namespace Sonarae.Cli.Configuration;

public sealed record AppConfiguration(string Style = "karaoke", int Lines = 1)
{
    public static string Path =>
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "sonarae",
            "config");

    public static AppConfiguration Load()
    {
        if (File.Exists(Path) == false)
            return new();

        var style = "karaoke";
        var lines = 1;

        foreach (var rawLine in File.ReadLines(Path))
        {
            var line = rawLine.Trim();

            if (line.StartsWith("style=", StringComparison.Ordinal))
            {
                style = line["style=".Length..]
                    .Trim()
                    .ToLowerInvariant();
                    
                continue;
            }

            if (line.StartsWith("lines=", StringComparison.Ordinal) &&
                int.TryParse(line["lines=".Length..].Trim(), out var parsedLines))
            {
                lines = Math.Clamp(parsedLines, 0, 5);
            }
        }

        return new(style, lines);
    }
}