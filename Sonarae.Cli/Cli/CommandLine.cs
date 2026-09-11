namespace Sonarae.Cli.Cli;

public sealed class CommandLine
{
    public bool ShowHelp { get; private set; }

    public string? Style { get; private set; }
    public int? Lines { get; private set; }
    public string? Error { get; private set; }

    public static CommandLine Parse(IReadOnlyList<string> args)
    {
        var result = new CommandLine();

        for (var i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "-h":
                case "--help":
                    result.ShowHelp = true;
                    break;

                case "-s":
                case "--style":
                    if (TryReadValue(
                            args,
                            ref i,
                            out var style,
                            out var error) == false)
                    {
                        return WithError(error);
                    }

                    result.Style =
                        style.ToLowerInvariant();

                    break;

                case "-l":
                case "--lines":
                    if (TryReadValue(
                            args,
                            ref i,
                            out var linesText,
                            out error) == false)
                    {
                        return WithError(error);
                    }

                    if (int.TryParse(
                            linesText,
                            out var lines) == false)
                    {
                        return WithError(
                            "--lines expects an integer.");
                    }

                    result.Lines =
                        Math.Clamp(lines, 0, 5);

                    break;

                default:
                    return WithError(
                        $"Unknown option: {args[i]}");
            }
        }

        return result;
    }

    private static bool TryReadValue(
        IReadOnlyList<string> args,
        ref int index,
        out string value,
        out string error)
    {
        if (index + 1 >= args.Count)
        {
            value = string.Empty;

            error =
                $"Missing value for {args[index]}.";

            return false;
        }

        value = args[++index];

        if (value.StartsWith('-'))
        {
            error =
                $"Missing value for {args[index - 1]}.";

            return false;
        }

        error = string.Empty;
        return true;
    }

    private static CommandLine WithError(string error) =>
        new()
        {
            Error = error
        };
}