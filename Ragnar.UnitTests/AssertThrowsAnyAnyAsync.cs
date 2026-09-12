namespace Ragnar.Tests;

public sealed partial class SummarizationStageTests
{
    private static class AssertThrowsAnyAnyAsync
    {
        public static Task<T> ThrowAnyAnyAsync<T>(Func<Task> func) where T : Exception
            => Task.FromResult<T>(null!);
    }
}
