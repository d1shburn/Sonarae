using System.Diagnostics;
using System.Globalization;

namespace Sonarae.Cli.Players.Mpris;

public sealed class MprisPlayer
{
    private string? _playerName;

    public async Task<MprisPlayback?> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        var player = await ResolvePlayerAsync(cancellationToken);

        if (player is null)
            return null;

        var metadata = await RunPlayerctlAsync(
            [
                "--player", player,
                "metadata",
                "--format",
                "{{artist}}\t{{title}}\t{{mpris:length}}"
            ],
            cancellationToken);

        if (string.IsNullOrWhiteSpace(metadata))
            return null;

        var parts = metadata.Trim().Split('\t');

        if (parts.Length < 3)
            return null;

        var artist = parts[0].Trim();
        var title = parts[1].Trim();

        if (title.Length == 0)
            return null;

        return new MprisPlayback(
            artist,
            title,
            await GetPositionAsync(player, cancellationToken),
            ParseMicroseconds(parts[2]));
    }

    private static async Task<TimeSpan> GetPositionAsync(
        string player,
        CancellationToken cancellationToken)
    {
        var output = await RunPlayerctlAsync(
            ["--player", player, "position"],
            cancellationToken);

        return double.TryParse(
            output?.Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var seconds)
            ? TimeSpan.FromSeconds(Math.Max(0, seconds))
            : TimeSpan.Zero;
    }

    private async Task<string?> ResolvePlayerAsync(
        CancellationToken cancellationToken)
    {
        var output = await RunPlayerctlAsync(["-l"], cancellationToken);

        if (string.IsNullOrWhiteSpace(output))
            return null;

        var players = output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static player => player.Trim())
            .Where(static player => player.Length > 0)
            .ToArray();

        if (players.Length == 0)
            return null;

        if (_playerName is not null &&
            players.Contains(_playerName, StringComparer.OrdinalIgnoreCase))
        {
            return _playerName;
        }

        foreach (var player in players)
        {
            var status = await RunPlayerctlAsync(
                ["--player", player, "status"],
                cancellationToken);

            if (status?.Trim().Equals(
                    "Playing",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                return _playerName = player;
            }
        }

        return _playerName = players[0];
    }

    private static async Task<string?> RunPlayerctlAsync(
        IEnumerable<string> arguments,
        CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "playerctl",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        try
        {
            if (process.Start() == false)
                return null;

            var output =
                await process.StandardOutput.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            return process.ExitCode == 0 ? output : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static TimeSpan ParseMicroseconds(string value)
    {
        if (long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var microseconds) == false)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromTicks(Math.Max(0, microseconds) * 10);
    }
}

public sealed record MprisPlayback(
    string Artist,
    string Title,
    TimeSpan Position,
    TimeSpan Duration)
{
    public string TrackKey =>
        $"{Artist}\u001f{Title}";
}