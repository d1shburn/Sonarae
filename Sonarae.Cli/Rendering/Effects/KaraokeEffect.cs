namespace Sonarae.Cli.Rendering.Effects;

public sealed class KaraokeEffect : ILyricEffect
{
    private static readonly Rgb Base =
        new(255, 104, 28);

    private static readonly Rgb Highlight =
        new(255, 222, 150);

    private static readonly Rgb Hot =
        new(255, 174, 58);

    public IReadOnlyList<Rgb> Render(string text, LyricEffectContext context)
    {
        var characters =
            text.EnumerateRunes().ToArray();

        if (characters.Length == 0)
            return [];

        var colors =
            new Rgb[characters.Length];

        var sweep =
            Math.Clamp(
                context.Progress * 1.35,
                0,
                1);

        var pulse =
            0.82 +
            0.18 *
            Math.Sin(
                context.Elapsed * 8.0);

        for (var i = 0; i < characters.Length; i++)
        {
            var normalized =
                characters.Length == 1
                    ? 0
                    : i / (double)(characters.Length - 1);

            var distance =
                Math.Abs(normalized - sweep);

            var glow =
                Math.Clamp(
                    1.0 - distance / 0.18,
                    0,
                    1);

            var baseColor =
                Rgb.Lerp(
                    Base,
                    Hot,
                    normalized * 0.7);

            var color =
                Rgb.Lerp(
                    baseColor,
                    Highlight,
                    glow * pulse);

            colors[i] = color;
        }

        return colors;
    }
}