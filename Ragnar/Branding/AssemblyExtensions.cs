namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
/// <remarks>Useful for dynamic version display in console headers and logs.</remarks>
/// <example><![CDATA[string v = asm.InformationalVersion;]]></example>
public static class AssemblyExtensions
{
    private static readonly ConcurrentDictionary<Assembly, string> _cache = new();


    /// <summary>Resolves and caches the informational version string of an assembly.</summary>
    private static string Resolve(Assembly asm) =>
        _cache.GetOrAdd(asm, a => a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
         ?? a.GetName().Version?.ToString()
         ?? "1.0.0");

    extension(Assembly asm)
    {
        /// <summary>Retrieves the informational version of *this* assembly (cached).</summary>
        /// <example><![CDATA[string v = Assembly.GetExecutingAssembly().InformationalVersion;]]></example>
        public string InformationalVersion => Resolve(asm);
    }
}
