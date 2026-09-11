namespace Sonarae.Cli.Rendering;

public static class Terminal
{
    public const string Reset = "\x1b[0m";

    public const string Accent =
        "\x1b[38;2;255;178;72m";

    public const string AccentBright =
        "\x1b[38;2;255;211;135m";

    public const string AccentSoft =
        "\x1b[38;2;255;145;45m";

    public const string Current =
        "\x1b[38;2;235;235;240m";

    public const string TextDim =
        "\x1b[38;2;82;82;90m";

    public const string EnterAlternateScreen =
        "\x1b[?1049h";

    public const string LeaveAlternateScreen =
        "\x1b[?1049l";

    public const string HideCursorSequence =
        "\x1b[?25l";

    public const string ShowCursorSequence =
        "\x1b[?25h";

    public const string DisableWrap =
        "\x1b[?7l";

    public const string EnableWrap =
        "\x1b[?7h";

    public const string ClearScreen =
        "\x1b[2J";

    public const string ClearLine =
        "\x1b[2K";

    public const string CursorHome =
        "\x1b[H";

    public static void Write(string text) =>
        Console.Write(text);

    public static void WriteLine(string text = "") =>
        Console.WriteLine(text);

    public static void WriteColored(
        string text,
        string color)
    {
        Console.Write(color);
        Console.Write(text);
        Console.Write(Reset);
    }

    public static void WriteAccent(string text) =>
        WriteColored(text, Accent);

    public static void WriteBoldColored(string text, string color)
    {
        Console.Write("\x1b[1m");
        Console.Write(color);
        Console.Write(text);
        Console.Write(Reset);
    }

    public static void WriteBoldSizedColored(
        string text,
        string color,
        int scale)
    {
        scale = Math.Clamp(scale, 1, 7);

        Console.Write("\x1b[1m");
        Console.Write(color);

        Console.Write(
            $"\x1b]66;s={scale};");

        Console.Write(text);

        Console.Write("\x07");

        Console.Write(Reset);
    }

    public static void WriteError(string text) =>
        Console.Error.WriteLine(
            $"\x1b[38;2;255;90;90m{text}{Reset}");

    public static void EnterScreen()
    {
        Console.Write(EnterAlternateScreen);

        Console.Write(DisableWrap);

        Console.Write(HideCursorSequence);

        Console.Write(CursorHome);

        Console.Write(ClearScreen);

        Console.Out.Flush();
    }

    public static void LeaveScreen()
    {
        Console.Write(Reset);

        Console.Write(EnableWrap);

        Console.Write(ShowCursorSequence);

        Console.Write(LeaveAlternateScreen);

        Console.Out.Flush();
    }

    public static void MoveCursor(int row, int column = 1) =>
        Console.Write(
            $"\x1b[{row};{column}H");

    public static void ClearCurrentLine() =>
        Console.Write(ClearLine);

    public static void Clear()
    {
        Console.Write(ClearScreen);

        Console.Write(CursorHome);
    }

    public static void Flush() =>
        Console.Out.Flush();
}

public readonly record struct Rgb(byte R, byte G, byte B)
{
    public string ToAnsi() =>
        $"\x1b[38;2;{R};{G};{B}m";

    public static Rgb Lerp(Rgb from, Rgb to, double amount)
    {
        amount = Math.Clamp(amount, 0d, 1d);

        return new Rgb(
            (byte)Math.Clamp(from.R + (to.R - from.R) * amount, 0, 255),

            (byte)Math.Clamp(from.G + (to.G - from.G) * amount, 0, 255),

            (byte)Math.Clamp(from.B + (to.B - from.B) * amount, 0, 255));
    }

    public Rgb Multiply(double amount)
    {
        amount = Math.Clamp(amount, 0d, 1d);

        return new Rgb(
            (byte)(R * amount),
            (byte)(G * amount),
            (byte)(B * amount));
    }
}