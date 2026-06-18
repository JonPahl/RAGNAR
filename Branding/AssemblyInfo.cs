using System.Reflection;

namespace Ragnar.Branding;

/// <summary>
/// Provides assembly information.
/// </summary>
public class AssemblyInfo : IAssemblyInfo
{
    /// <summary>
    /// Gets the current assembly.
    /// </summary>
    public Assembly Assembly => Assembly.GetExecutingAssembly();
}
