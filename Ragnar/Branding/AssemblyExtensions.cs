namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
public static class AssemblyExtensions
{
    extension(IAssemblyInfo asm)
    {
        /// <summary>Gets assembly's informational version string.</summary>
        /// <returns>Version string or "1.0.0" if missing.</returns>
        /// <example><![CDATA[string v = AssemblyExtensions.InformationalVersion(asm);]]></example>
        public string? InformationalVersion => asm.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "1.0.0";
    }
}
