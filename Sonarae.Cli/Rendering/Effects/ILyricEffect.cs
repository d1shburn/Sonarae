namespace Sonarae.Cli.Rendering.Effects;

public interface ILyricEffect
{
    IReadOnlyList<Rgb> Render(string text, LyricEffectContext context);
}