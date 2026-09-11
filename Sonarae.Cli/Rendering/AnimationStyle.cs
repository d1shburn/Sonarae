namespace Sonarae.Cli.Rendering;

public enum AnimationStyle
{
    Karaoke,
    Focus
}

public static class AnimationStyles
{
    public static IReadOnlyList<string> All { get; } =
    [
        "karaoke",
        "focus"
    ];

    public static AnimationStyle? Resolve(string name) =>
        name.ToLowerInvariant() switch
        {
            "karaoke" => AnimationStyle.Karaoke,
            "focus" => AnimationStyle.Focus,
            _ => null
        };
}