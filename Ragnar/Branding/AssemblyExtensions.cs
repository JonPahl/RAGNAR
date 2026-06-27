
using System.Reflection;

namespace Ragnar.Branding;

/// <summary>
/// Gets the informational version of an assembly.
/// </summary>
public static class AssemblyExtensions
{
    extension(IAssemblyInfo asm)
    {
        /// <summary>
        /// Gets the informational version of an assembly.
        /// </summary>
        /// <returns>The informational version as a string, or null if not found.</returns>
        /// <example>
        /// <![CDATA[ var asm = new MyAssembly();
        /// string? infoVersion = AssemblyExtensions.InformationalVersion(asm); ]]>
        /// </example>
        public string? InformationalVersion => asm.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
    }
}
