using Sonarae.Cli.Cli;

using Sonarae.Cli.Configuration;

using Sonarae.Cli.Players.Mpris;

using Sonarae.Cli.Rendering;
using Sonarae.Cli.Rendering.Effects;
using Sonarae.Cli.Rendering.Output;

namespace Sonarae.Cli.Core;

public sealed class Application
{
    public static async Task<int> RunAsync(
        string[] args)
    {
        var commandLine =
            CommandLine.Parse(args);

        if (commandLine.Error is not null)
        {
            Terminal.WriteError(
                commandLine.Error);

            Terminal.WriteLine();

            HelpScreen.Show();
            return 1;
        }

        if (commandLine.ShowHelp)
        {
            HelpScreen.Show();
            return 0;
        }

        var configuration =
            AppConfiguration.Load();

        var styleName =
            commandLine.Style ??
            configuration.Style;

        var lines =
            commandLine.Lines ??
            configuration.Lines;

        if (AnimationStyles.Resolve(styleName)
            is not { } style)
        {
            Terminal.WriteError(
                $"Unknown animation style: {styleName}");

            Terminal.WriteLine(
                "Use 'sonarae --help' to see available styles.");

            return 1;
        }

        var radius = Math.Clamp(lines, 0, 5);

        var player = new MprisPlayer();
        var lyricsProvider = new SyncedLyricsProvider();
        var layout = new TerminalLayout();
        var writer = new TerminalWriter(layout);
        var renderer = new LiveLyricsRenderer(writer);

        using var cancellationSource = new CancellationTokenSource();

        Console.CancelKeyPress +=
            (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
            };

        try
        {
            await renderer.RunAsync(
                player,
                lyricsProvider,
                style,
                radius,
                cancellationSource.Token);
        }
        catch (OperationCanceledException)
            when (cancellationSource.IsCancellationRequested)
        {
            return 0;
        }

        return 0;
    }
}