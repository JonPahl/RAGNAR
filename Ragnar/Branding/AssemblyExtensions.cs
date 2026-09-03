namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Assembly extension method.
    /// </summary>
    /// <param name="asm">Assembly Info.</param>
    extension(Assembly asm)
    {
        /// <summary>Retrieves the informational version from an assembly.</summary>
        /// <returns>The informational version string or default fallback.</returns>
        public string? InformationalVersion => asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    }
}
