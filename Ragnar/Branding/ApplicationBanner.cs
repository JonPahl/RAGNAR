namespace Ragnar.Branding;

/// <summary>
/// Setups up and displays Branding for Ragnar application.
/// </summary>
/// <param name="writer">console output library.</param>
/// <param name="assemblyInfo">AssemblyInfo interface.</param>
public sealed class ApplicationBanner(IOutputWriter writer, IAssemblyInfo assemblyInfo)
    : IApplicationBanner
{
    /// <summary>
    /// Gets current assembly version.
    /// </summary>
    private readonly string versionNumber = assemblyInfo.InformationalVersion ?? "1.0.0";

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

        writer.Write(appName);
        writer.Write(new Text(title + " (Repository Augmented Generator & Resolver)", Styles.BoldBlue));
        writer.Write(version);
        writer.WriteLine();
        writer.Write(tagLine2);
        writer.WriteLine();
        writer.WriteRule();
        writer.WriteLine();
    }
}
