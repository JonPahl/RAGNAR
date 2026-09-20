namespace Ragnar.Output;

public interface IClock
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
