using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Sonarae.Cli.Lyrics;

namespace Sonarae.Cli.Players.Mpris;

public sealed class SyncedLyricsProvider
{
    private static readonly Uri LrcMuxEndpoint =
        new("https://api.lrcmux.dev/get");

    private static readonly Uri LrcLibEndpoint =
        new("https://lrclib.net/api/get");

    private readonly HttpClient _httpClient;

    private string? _cachedTrackKey;
    private LyricsDocument? _cachedLyrics;

    public SyncedLyricsProvider(HttpClient? httpClient = null) =>
        _httpClient = httpClient ?? CreateHttpClient();

    public async Task<LyricsDocument> GetAsync(
        string artist,
        string title,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var key =
            $"{artist}\u001f{title}\u001f{duration.TotalSeconds:F0}";

        if (key == _cachedTrackKey && _cachedLyrics is not null)
            return _cachedLyrics;

        var lyrics = await TryLrcMuxAsync(
            artist,
            title,
            duration,
            cancellationToken);

        if (lyrics.Lines.Count == 0)
        {
            lyrics = await TryLrcLibAsync(
                artist,
                title,
                duration,
                cancellationToken);
        }

        if (lyrics.Lines.Count > 0)
        {
            _cachedTrackKey = key;
            _cachedLyrics = lyrics;
        }

        return lyrics;
    }

    private async Task<LyricsDocument> TryLrcMuxAsync(
        string artist,
        string title,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(
                LrcMuxEndpoint,
                ("artist", artist),
                ("title", title),
                ("duration", duration.TotalSeconds.ToString(
                    "F0",
                    CultureInfo.InvariantCulture)));

            var response =
                await _httpClient.GetFromJsonAsync<LrcMuxResponse>(
                    url,
                    cancellationToken);

            if (response?.Meta?.Instrumental == true ||
                response?.Lines is null)
            {
                return LyricsDocument.Empty;
            }

            var lines = response.Lines
                .Where(static line =>
                    line.Start.HasValue &&
                    string.IsNullOrWhiteSpace(line.Text) is false)
                .Select(static line =>
                {
                    var words = line.Words?
                        .Where(static word =>
                            word.Start >= 0 &&
                            string.IsNullOrWhiteSpace(word.Text) is false)
                        .Select(static word =>
                            new LyricWord(
                                TimeSpan.FromMilliseconds(
                                    Math.Max(0, word.Start)),
                                word.Text!.Trim()))
                        .OrderBy(static word => word.Timestamp)
                        .ToArray();

                    return new LyricLine(
                        TimeSpan.FromMilliseconds(
                            Math.Max(
                                0,
                                line.Start.GetValueOrDefault())),
                        line.Text!.Trim(),
                        words is { Length: > 0 } ? words : null);
                })
                .OrderBy(static line => line.Timestamp)
                .ToList();

            return lines.Count > 0
                ? new LyricsDocument(lines)
                : LyricsDocument.Empty;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return LyricsDocument.Empty;
        }
        catch (TaskCanceledException)
        {
            return LyricsDocument.Empty;
        }
        catch (JsonException)
        {
            return LyricsDocument.Empty;
        }
    }

    private async Task<LyricsDocument> TryLrcLibAsync(
        string artist,
        string title,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(
                LrcLibEndpoint,
                ("artist_name", artist),
                ("track_name", title),
                ("duration", duration.TotalSeconds.ToString(
                    "F0",
                    CultureInfo.InvariantCulture)));

            var response =
                await _httpClient.GetFromJsonAsync<LrcLibResponse>(
                    url,
                    cancellationToken);

            var syncedLyrics = response?.SyncedLyrics;

            return string.IsNullOrWhiteSpace(syncedLyrics)
                ? LyricsDocument.Empty
                : ParseLrc(syncedLyrics);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return LyricsDocument.Empty;
        }
        catch (TaskCanceledException)
        {
            return LyricsDocument.Empty;
        }
        catch (JsonException)
        {
            return LyricsDocument.Empty;
        }
    }

    private static LyricsDocument ParseLrc(string content)
    {
        var lines = new List<LyricLine>();

        foreach (var rawLine in content.Split(
                     '\n',
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            var index = 0;

            while (index < line.Length && line[index] == '[')
            {
                var closing = line.IndexOf(']', index);

                if (closing <= index ||
                    TryParseTimestamp(
                        line[(index + 1)..closing],
                        out var timestamp) is false)
                {
                    break;
                }

                var text =
                    line[(closing + 1)..].Trim();

                if (text.Length > 0)
                {
                    lines.Add(
                        new LyricLine(
                            timestamp,
                            text));
                }

                index = closing + 1;
            }
        }

        return lines.Count == 0
            ? LyricsDocument.Empty
            : new LyricsDocument(
                lines
                    .OrderBy(static line => line.Timestamp)
                    .ToList());
    }

    private static bool TryParseTimestamp(
        string value,
        out TimeSpan timestamp)
    {
        timestamp = TimeSpan.Zero;

        var separator = value.IndexOf(':');

        if (separator <= 0)
            return false;

        if (int.TryParse(
                value[..separator],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var minutes) is false)
        {
            return false;
        }

        if (double.TryParse(
                value[(separator + 1)..],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var seconds) is false)
        {
            return false;
        }

        timestamp =
            TimeSpan.FromMinutes(minutes) +
            TimeSpan.FromSeconds(seconds);

        return true;
    }

    private static string BuildUrl(
        Uri endpoint,
        params (string Name, string Value)[] parameters)
    {
        var query = string.Join(
            '&',
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}=" +
                $"{Uri.EscapeDataString(parameter.Value)}"));

        return $"{endpoint}?{query}";
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Sonarae/0.1.0");

        return client;
    }

    private sealed class LrcMuxResponse
    {
        [JsonPropertyName("meta")]
        public LrcMuxMeta? Meta { get; set; }

        [JsonPropertyName("lines")]
        public List<LrcMuxLine>? Lines { get; set; }
    }

    private sealed class LrcMuxMeta
    {
        [JsonPropertyName("instrumental")]
        public bool Instrumental { get; set; }
    }

    private sealed class LrcMuxLine
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("start")]
        public long? Start { get; set; }

        [JsonPropertyName("words")]
        public List<LrcMuxWord>? Words { get; set; }
    }

    private sealed class LrcMuxWord
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("end")]
        public long? End { get; set; }
    }

    private sealed class LrcLibResponse
    {
        [JsonPropertyName("syncedLyrics")]
        public string? SyncedLyrics { get; set; }
    }
}