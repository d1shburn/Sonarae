namespace Sonarae.Cli.Rendering;

public readonly record struct TerminalSize(int Width, int Height);

public sealed class TerminalLayout
{
    private TerminalSize _lastSize;

    public static TerminalSize Size =>
        ReadSize();

    public bool HasChanged()
    {
        var current = Size;

        if (current == _lastSize)
            return false;

        _lastSize = current;
        return true;
    }

    public static int GetLyricsStartRow(int lineCount)
    {
        var height = Math.Max(1, Size.Height);

        return Math.Max(1, (height - lineCount) / 2 + 1);
    }

    public static int GetFooterRow(int lyricsLineCount)
    {
        var start = GetLyricsStartRow(lyricsLineCount);

        return Math.Min(Size.Height, start + lyricsLineCount + 2);
    }

    private static TerminalSize ReadSize()
    {
        try
        {
            return new TerminalSize(
                Math.Max(1, Console.WindowWidth),
                Math.Max(1, Console.WindowHeight));
        }
        catch
        {
            return new TerminalSize(80, 24);
        }
    }
}