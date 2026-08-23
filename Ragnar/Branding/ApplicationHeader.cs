using Ragnar.Contracts;

namespace Ragnar.Branding;

/// <summary>Initializes a new instance of the application header.</summary>
/// <param name = "Writer"> Console output writer for branding display.</param>
/// <param name = "AssemblyInfo"> Assembly information provider.</param>
public sealed class ApplicationHeader(IOutputWriter Writer, IAssemblyInfo AssemblyInfo)
    : IApplicationHeader
{

    /// <summary>Caches the current assembly informational version string.</summary>
    private readonly string versionNumber = AssemblyInfo.InformationalVersion ?? "1.0.0";

    /// <summary>
    /// RenderBranding branding banner onto the console UI.
    /// </summary>
    public void RenderBranding()
    {
        const string title = "Ragnar";
        const string tagLine = "Smart, recursive code reasoning — from query to solution.";

        var appName = new Text(title, Styles.Blue) { Justification = Justify.Left };
        var tagLine2 = new Text(tagLine, Styles.BoldSteelBlue) { Justification = Justify.Center };

        var versionText = $"Version {versionNumber}";
        var version = new Text(versionText, new Style(Color.Grey)) { Justification = Justify.Center };

        Writer.Write(appName);
        Writer.Write(new Text(title + " (Repository Augmented Generator & Resolver)", Styles.BoldBlue));
        Writer.Write(version);
        Writer.WriteLine();
        Writer.Write(tagLine2);
        Writer.WriteLine();
        Writer.WriteRule();
        Writer.WriteLine();
    }
}
