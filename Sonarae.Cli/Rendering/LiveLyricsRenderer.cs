using Sonarae.Cli.Lyrics;
using Sonarae.Cli.Players.Mpris;
using Sonarae.Cli.Rendering.Effects;
using Sonarae.Cli.Rendering.Output;

namespace Sonarae.Cli.Rendering;

public sealed class LiveLyricsRenderer
{
    private readonly TerminalWriter _writer;

    public LiveLyricsRenderer(TerminalWriter writer) =>
        _writer = writer;

    public async Task RunAsync(
        MprisPlayer player,
        SyncedLyricsProvider lyricsProvider,
        AnimationStyle style,
        int radius,
        CancellationToken cancellationToken = default)
    {
        var viewport = new LyricsViewport(radius);
        var effect = LyricEffectFactory.Create(style);
        var trackKey = string.Empty;
        var lyrics = LyricsDocument.Empty;

        TerminalWriter.Enter();

        try
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                _writer.HandleResize();

                var playback =
                    await player.GetCurrentAsync(cancellationToken);

                if (playback is null)
                {
                    TerminalWriter.ClearFrame();

                    TerminalWriter.DrawWaiting(
                        "Waiting for player...");

                    TerminalWriter.Flush();

                    await Task.Delay(250, cancellationToken);
                    continue;
                }

                if (string.Equals(
                        playback.TrackKey,
                        trackKey,
                        StringComparison.Ordinal) == false)
                {
                    trackKey = playback.TrackKey;

                    lyrics =
                        await lyricsProvider.GetAsync(
                            playback.Artist,
                            playback.Title,
                            playback.Duration,
                            cancellationToken);

                    TerminalWriter.ClearFrame();
                }

                if (style == AnimationStyle.Focus)
                    RenderFocusFrame(playback, lyrics);
                else
                    RenderFrame(playback, lyrics, viewport, effect);

                TerminalWriter.Flush();

                await Task.Delay(33, cancellationToken);
            }
        }
        finally
        {
            TerminalWriter.Exit();
        }
    }

    private static void RenderFocusFrame(
        MprisPlayback playback,
        LyricsDocument lyrics)
    {
        TerminalWriter.ClearFrame();

        var lineIndex = lyrics.FindCurrentIndex(playback.Position);

        if (lineIndex < 0)
        {
            TerminalWriter.DrawWaiting(
                "Waiting for lyrics...");

            return;
        }

        var line = lyrics.Lines[lineIndex];

        if (line.Words is not { Count: > 0 } words)
        {
            TerminalWriter.DrawWaiting(
                "Word sync unavailable");

            return;
        }

        var wordIndex = FindCurrentWordIndex(words, playback.Position);

        if (wordIndex < 0)
        {
            TerminalWriter.DrawWaiting(
                "Waiting for word...");

            return;
        }

        var word = words[wordIndex].Text;

        TerminalWriter.DrawFocusLine(
            Math.Max(1, TerminalWriter.Size.Height / 2), word);
    }

    private static int FindCurrentWordIndex(
        IReadOnlyList<LyricWord> words,
        TimeSpan position)
    {
        var low = 0;
        var high = words.Count - 1;
        var result = -1;

        while (low <= high)
        {
            var middle =
                low + (high - low) / 2;

            if (words[middle].Timestamp <= position)
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

    private static void RenderFrame(
        MprisPlayback playback,
        LyricsDocument lyrics,
        LyricsViewport viewport,
        ILyricEffect effect)
    {
        var currentIndex =
            lyrics.FindCurrentIndex(
                playback.Position);

        var visible =
            currentIndex >= 0
                ? viewport.GetLines(
                    lyrics,
                    currentIndex)
                : Enumerable
                    .Repeat<LyricLine?>(
                        null,
                        viewport.TotalLines)
                    .ToArray();

        var startRow =
            Math.Max(1, (TerminalWriter.Size.Height - visible.Count) / 2 + 1);

        TerminalWriter.ClearRows(startRow, visible.Count);

        for (var i = 0; i < visible.Count; i++)
        {
            var line = visible[i];

            if (line is null)
                continue;

            var distance = Math.Abs(i - viewport.Radius);

            var row = startRow + i;

            if (distance == 0 && currentIndex >= 0)
            {
                DrawCurrent(
                    row,
                    line,
                    lyrics,
                    currentIndex,
                    playback,
                    effect);

                continue;
            }

            DrawContext(row, line, distance);
        }
    }

    private static void DrawCurrent(
        int row,
        LyricLine line,
        LyricsDocument lyrics,
        int currentIndex,
        MprisPlayback playback,
        ILyricEffect effect)
    {
        var nextTimestamp =
            currentIndex + 1 < lyrics.Lines.Count
                ? lyrics.Lines[currentIndex + 1].Timestamp
                : playback.Duration;

        var context =
            new LyricEffectContext(
                playback.Position,
                line.Timestamp,
                nextTimestamp,
                TimeSpan.Zero);

        var colors =
            effect.Render(
                line.Text,
                context);

        TerminalWriter.DrawGradientLine(
            row,
            line.Text,
            colors);
    }

    private static void DrawContext(
        int row,
        LyricLine line,
        int distance)
    {
        var color =
            distance switch
            {
                1 => new Rgb(108, 82, 65),
                2 => new Rgb(68, 57, 50),
                3 => new Rgb(44, 40, 38),
                _ => new Rgb(32, 31, 30)
            };

        TerminalWriter.DrawLine(
            row,
            line.Text,
            color);
    }
}