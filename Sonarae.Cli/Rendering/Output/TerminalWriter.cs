using System.Globalization;
using System.Text;

using Sonarae.Cli.Rendering;

namespace Sonarae.Cli.Rendering.Output;

public sealed class TerminalWriter
{
    private readonly TerminalLayout _layout;

    public TerminalWriter(TerminalLayout layout) =>
        _layout = layout;

    public static TerminalSize Size =>
        TerminalLayout.Size;

    public void HandleResize()
    {
        if (_layout.HasChanged())
            ClearFrame();
    }

    public static void Enter()
    {
        Terminal.Write(Terminal.EnterAlternateScreen);
        Terminal.Write(Terminal.HideCursorSequence);
        Terminal.Write(Terminal.DisableWrap);
        Terminal.Write(Terminal.ClearScreen);
        Terminal.Write(Terminal.CursorHome);
    }

    public static void Exit()
    {
        Terminal.Write(Terminal.Reset);
        Terminal.Write(Terminal.EnableWrap);
        Terminal.Write(Terminal.ShowCursorSequence);
        Terminal.Write(Terminal.LeaveAlternateScreen);
    }

    public static void ClearFrame()
    {
        Terminal.Write(Terminal.ClearScreen);
        Terminal.Write(Terminal.CursorHome);
    }

    public static void ClearRows(int startRow, int count)
    {
        if (count <= 0)
            return;

        for (var row = startRow; row < startRow + count; row++)
        {
            Terminal.MoveCursor(row, 1);

            Terminal.Write(Terminal.ClearLine);
        }
    }

    public static void DrawLine(int row, string text, Rgb color)
    {
        var clipped = Clip(text, Size.Width);

        if (clipped.Length == 0)
            return;

        var column = CenterColumn(clipped);

        Terminal.MoveCursor(row, column);

        Terminal.WriteColored(clipped, color.ToAnsi());
    }

    public static void DrawFocusLine(int centerRow, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var upper = text.Trim().ToUpperInvariant();

        if (upper.Length == 0)
            return;

        var normalWidth = DisplayWidth(upper);

        if (normalWidth <= 0)
            return;

        var scale = CalculateFocusScale(normalWidth, Size.Width);

        var renderedWidth = normalWidth * scale;

        var column = Math.Max(1, (Size.Width - renderedWidth) / 2 + 1);

        var topRow = Math.Max(1, centerRow - scale / 2);

        Terminal.MoveCursor(topRow, column);

        Terminal.Write("\x1b[1m");

        Terminal.Write(Terminal.AccentBright);

        Terminal.Write($"\x1b]66;s={scale};{upper}\x07");

        Terminal.Write(Terminal.Reset);
    }

    public static void DrawBoldCentered(int row, string text, Rgb color)
    {
        var clipped = Clip(text, Size.Width);

        if (clipped.Length == 0)
            return;

        var column = CenterColumn(clipped);

        Terminal.MoveCursor(row, column);

        Terminal.WriteBoldColored(clipped, color.ToAnsi());
    }

    public static void DrawGradientLine(
        int row,
        string text,
        IReadOnlyList<Rgb> colors)
    {
        if (string.IsNullOrEmpty(text) || colors.Count == 0)
            return;

        var clipped = Clip(text, Size.Width);

        if (clipped.Length == 0)
            return;

        var column = CenterColumn(clipped);

        Terminal.MoveCursor(row, column);

        var runes = clipped.EnumerateRunes().ToArray();

        var length = Math.Min(runes.Length, colors.Count);

        for (var i = 0; i < length; i++)
            Terminal.WriteColored(runes[i].ToString(), colors[i].ToAnsi());
    }

    public static void DrawWaiting(string text)
    {
        var row = Math.Max(1, Size.Height / 2);

        DrawLine(row, text, new Rgb(82, 82, 90));
    }

    public static void Flush() =>
        Console.Out.Flush();

    private static int CalculateFocusScale(int textWidth, int terminalWidth)
    {
        if (textWidth <= 0 || terminalWidth <= 0)
            return 1;

        var availableWidth =
            Math.Max(1, terminalWidth - 4);

        var scale = availableWidth / textWidth;

        return Math.Clamp(scale, 1, 7);
    }

    private static int CenterColumn(string text)
    {
        var width = DisplayWidth(text);

        return Math.Max(1, (Size.Width - width) / 2 + 1);
    }

    private static string Clip(string text, int width)
    {
        if (width <= 0 || string.IsNullOrEmpty(text))
            return string.Empty;

        var result = new StringBuilder();

        var currentWidth = 0;

        foreach (var rune in text.EnumerateRunes())
        {
            var runeWidth = RuneDisplayWidth(rune);

            if (currentWidth + runeWidth > width)
                break;

            result.Append(rune.ToString());

            currentWidth += runeWidth;
        }

        return result.ToString();
    }

    private static int DisplayWidth(string text)
    {
        var width = 0;

        foreach (var rune in text.EnumerateRunes())
            width += RuneDisplayWidth(rune);

        return width;
    }

    private static int RuneDisplayWidth(System.Text.Rune rune)
    {
        var category = System.Text.Rune.GetUnicodeCategory(rune);

        if (category is
            UnicodeCategory.NonSpacingMark or
            UnicodeCategory.EnclosingMark)
        {
            return 0;
        }

        return 1;
    }
}