namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
/// <remarks>Useful for dynamic version display in console headers and logs.</remarks>
/// <example><![CDATA[string v = asm.InformationalVersion;]]></example>
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
        // <example><![CDATA[[string v = typeof(App).Assembly.InformationalVersion;]]></example>
        public string? InformationalVersion => asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    }
}
