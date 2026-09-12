namespace Ragnar.Tests.Helper;


public static class AssertExt
{
    public static async Task<T> OrAwait<T>(this T val, Func<Task> alt) => val;
}
