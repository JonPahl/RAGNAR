namespace Ragnar.Output;

public record SystemClock : IClock
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}
