using Sonarae.Cli.Rendering;

namespace Sonarae.Cli.Cli;

public static class HelpScreen
{
    private const int FrameWidth = 48;

    public static void Show()
    {
        Terminal.Clear();

        Terminal.WriteLine();

        Header();

        Section("USAGE");

        Terminal.Write("    ");

        Terminal.WriteColored(
            "sonarae",
            Terminal.AccentBright);

        Terminal.Write(" ");

        Terminal.WriteColored(
            "[OPTIONS]",
            Terminal.TextDim);

        Terminal.WriteLine();

        Terminal.WriteLine();

        Section("OPTIONS");

        Option(
            "-s, --style <STYLE>",
            "Select animation style");

        Option(
            "-l, --lines <COUNT>",
            "Lines above and below current");

        Option(
            "-h, --help",
            "Show this help");

        Terminal.WriteLine();

        Section("STYLES");

        Style(
            "karaoke",
            "Animated gradient lyrics");

        Style(
            "focus",
            "One word at a time with word sync");

        Terminal.WriteLine();

        Section("EXAMPLES");

        Example(
            "sonarae");

        Example(
            "sonarae --style karaoke");

        Example(
            "sonarae --style karaoke --lines 3");

        Example(
            "sonarae --style focus");
    }

    public static void ShowError(
        string message) =>
        Terminal.WriteError(message);

    private static void Header()
    {
        Terminal.WriteLine(
            $"  {Terminal.Accent}┌" +
            new string('─', FrameWidth) +
            $"┐{Terminal.Reset}");

        Terminal.WriteLine(
            $"  {Terminal.Accent}│" +
            $"{Terminal.Reset}" +
            new string(' ', FrameWidth) +
            $"{Terminal.Accent}│" +
            $"{Terminal.Reset}");

        Terminal.WriteLine(
            $"  {Terminal.Accent}│" +
            $"{Terminal.Reset}  " +
            $"{Terminal.AccentBright}SONARAE" +
            $"{Terminal.Reset}" +
            new string(
                ' ',
                FrameWidth - 9) +
            $"{Terminal.Accent}│" +
            $"{Terminal.Reset}");

        Terminal.WriteLine(
            $"  {Terminal.Accent}│" +
            $"{Terminal.Reset}  " +
            $"{Terminal.TextDim}" +
            "Terminal lyrics utility" +
            $"{Terminal.Reset}" +
            new string(
                ' ',
                FrameWidth - 25) +
            $"{Terminal.Accent}│" +
            $"{Terminal.Reset}");

        Terminal.WriteLine(
            $"  {Terminal.Accent}│" +
            $"{Terminal.Reset}" +
            new string(' ', FrameWidth) +
            $"{Terminal.Accent}│" +
            $"{Terminal.Reset}");

        Terminal.WriteLine(
            $"  {Terminal.Accent}└" +
            new string('─', FrameWidth) +
            $"┘{Terminal.Reset}");

        Terminal.WriteLine();
    }

    private static void Section(
        string title)
    {
        Terminal.Write("  ");

        Terminal.WriteColored(
            title,
            Terminal.AccentBright);

        Terminal.Write("  ");

        Terminal.WriteLine(
            $"{Terminal.TextDim}" +
            new string(
                '─',
                FrameWidth - title.Length - 4) +
            $"{Terminal.Reset}");

        Terminal.WriteLine();
    }

    private static void Option(
        string option,
        string description)
    {
        Terminal.Write("    ");

        Terminal.WriteColored(
            option.PadRight(23),
            Terminal.AccentBright);

        Terminal.WriteLine(
            $"{Terminal.TextDim}" +
            description +
            $"{Terminal.Reset}");
    }

    private static void Style(
        string name,
        string description)
    {
        Terminal.Write("    ");

        Terminal.WriteColored(
            $"{name,-19}",
            Terminal.AccentBright);

        Terminal.WriteLine(
            $"{Terminal.TextDim}" +
            description +
            $"{Terminal.Reset}");
    }

    private static void Example(
        string command)
    {
        Terminal.Write("    ");

        Terminal.WriteColored(
            command,
            Terminal.AccentBright);

        Terminal.WriteLine();
    }
}