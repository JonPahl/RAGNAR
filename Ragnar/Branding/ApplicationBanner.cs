namespace Ragnar.Branding;

/// <summary>Displays application branding and version info.</summary>
/// <param name="Writer">Console output writer.</param>
/// <param name="AssemblyInfo">Assembly metadata provider.</param>
/// <example><![CDATA[RenderBranding()]]></example>
public sealed class ApplicationBanner(IOutputWriter Writer, IAssemblyInfo AssemblyInfo) : IApplicationBanner
{
    /// <summary>
    /// Gets current assembly version.
    /// </summary>
    private readonly string VersionNumber = AssemblyInfo.InformationalVersion ?? "1.0.0";

    /// <summary>
    /// RenderBranding branding banner onto the console UI.
    /// </summary>
    public void RenderBranding()
    {
        const string Title = "Ragnar";
        const string TagLine = "Smart, recursive code reasoning — from query to solution.";

        var AppName = new Text(Title, Styles.Blue) { Justification = Justify.Left };
        var TagLine2 = new Text(TagLine, Styles.BoldSteelBlue) { Justification = Justify.Center };

        var VersionText = $"Version {VersionNumber}";
        var Version = new Text(VersionText, new Style(Color.Grey)) { Justification = Justify.Center };

        Writer.Write(AppName);
        Writer.Write(new Text(Title + " (Repository Augmented Generator & Resolver)", Styles.BoldBlue));
        Writer.Write(Version);
        Writer.WriteLine();
        Writer.Write(TagLine2);
        Writer.WriteLine();
        Writer.WriteRule();
        Writer.WriteLine();
    }
}
