namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
/// <remarks>Useful for dynamic version display in console headers and logs.</remarks>
/// <example><![CDATA[string v = asm.InformationalVersion;]]></example>
public static class AssemblyExtensions
{
    /// <summary>Adds version-query helpers to <see cref="Assembly"/> instances.</summary>
    /// <param name="asm">The target assembly to extend.</param>
    /// <example><![CDATA[var v = typeof(App).Assembly.InformationalVersion;]]></example>
    extension(Assembly asm)
    {
        /// <summary>Retrieves the informational version from an assembly.</summary>
        /// <returns>The informational version string or default fallback.</returns>
        /// <example><![CDATA[string v = typeof(App).Assembly.InformationalVersion;]]></example>
        public string InformationalVersion => "1.0.0";
        // cachedVersion.Version;

        //private string cachedVersion = new(() =>
        //typeof(AssemblyExtensions).Assembly
        //.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0");
    }
}
