namespace Sonarae.Cli.Lyrics;

public sealed class LyricsDocument
{
    public static LyricsDocument Empty { get; } = new([]);

    public IReadOnlyList<LyricLine> Lines { get; }

    public LyricsDocument(IReadOnlyList<LyricLine> lines) =>
        Lines = lines.ToArray();

    public int FindCurrentIndex(TimeSpan position)
    {
        var low = 0;
        var high = Lines.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var middle = low + (high - low) / 2;

            if (Lines[middle].Timestamp <= position)
            {
                result = middle;
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return result;
    }
}