namespace Ragnar.IntegrationTests;

public static class PathExtensions
{
    extension(string? path)
    {
        public string ExpandDirectory ()
        {
            if(path is null) throw new ArgumentNullException(nameof(path));
            return Environment.ExpandEnvironmentVariables(path);
        }
    }
}
