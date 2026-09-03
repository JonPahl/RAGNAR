
using System.Reflection;

namespace Ragnar.Branding;

/// <summary>Provides assembly information.</summary>
public interface IAssemblyInfo
{
    /// <summary>Gets the current assembly.</summary>
    Assembly Assembly { get; }
}
