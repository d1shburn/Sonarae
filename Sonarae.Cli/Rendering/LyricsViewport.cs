using Sonarae.Cli.Lyrics;

namespace Sonarae.Cli.Rendering;

public sealed class LyricsViewport(int radius)
{
    public int Radius { get; } = Math.Clamp(radius, 0, 5);

    public int TotalLines => Radius * 2 + 1;

    public IReadOnlyList<LyricLine?> GetLines(
        LyricsDocument lyrics,
        int currentIndex)
    {
        var result = new LyricLine?[TotalLines];

        for (var slot = 0; slot < result.Length; slot++)
        {
            var index = currentIndex + slot - Radius;

            if ((uint)index < (uint)lyrics.Lines.Count)
                result[slot] = lyrics.Lines[index];
        }

        return result;
    }
}