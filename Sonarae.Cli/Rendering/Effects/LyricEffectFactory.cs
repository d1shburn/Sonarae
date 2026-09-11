namespace Sonarae.Cli.Rendering.Effects;

public static class LyricEffectFactory
{
    public static ILyricEffect Create(AnimationStyle style) =>
        style switch
        {
            AnimationStyle.Karaoke => new KaraokeEffect(),
            _ => new KaraokeEffect()
        };
}
