namespace Sonarae.Cli.Rendering.Effects;

public readonly record struct LyricEffectContext(
    TimeSpan Position,
    TimeSpan Timestamp,
    TimeSpan NextTimestamp,
    TimeSpan FrameTime)
{
    public double Elapsed =>
        Math.Max(0, (Position - Timestamp).TotalSeconds);

    public double Progress
    {
        get
        {
            var duration =
                (NextTimestamp - Timestamp).TotalSeconds;

            if (duration <= 0)
                return 1;

            return Math.Clamp(
                Elapsed / duration,
                0,
                1);
        }
    }

    public double Pulse(double speed, double phase = 0) =>
        0.5 + 0.5 * Math.Sin(Elapsed * speed + phase);
}