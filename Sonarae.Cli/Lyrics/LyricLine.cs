namespace Sonarae.Cli.Lyrics;

public sealed record LyricLine(
    TimeSpan Timestamp,
    string Text,
    IReadOnlyList<LyricWord>? Words = null);