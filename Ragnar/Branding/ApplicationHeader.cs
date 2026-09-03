namespace Ragnar.Branding;

/// <summary>Renders application branding and version information to console.</summary>
/// <param name="writer">Console output writer used for branding display.</param>
/// <remarks>Caches version string on initialization for rendering performance.</remarks>
public sealed class ApplicationHeader(IOutputWriter writer)
    : IApplicationHeader
{

    /// <summary>Caches the current assembly informational version string.</summary>
    private readonly string _versionNumber =
Assembly.GetExecutingAssembly().InformationalVersion ?? "1.0.0";

    /// <summary>Renders Ragnar title, tagline, and version to console.</summary>
    /// <remarks>Outputs styled text and a horizontal rule via the configured writer instance.</remarks>
    /// <example><![CDATA[header.RenderBranding();]]></example>
    public void RenderBranding()
    {
        const string TITLE = "Ragnar";
        const string TAGLINE = "Smart, recursive code reasoning — from query to solution.";

        var appName = new Text(TITLE, Styles.Blue) { Justification = Justify.Left };
        var tagLine2 = new Text(TAGLINE, Styles.BoldSteelBlue) { Justification = Justify.Center };

        var versionText = $"Version {_versionNumber}";
        var version = new Text(versionText, new Style(Color.Grey)) { Justification = Justify.Center };

        writer.Write(appName);
        writer.Write(new Text(TITLE + " (Repository Augmented Generator & Resolver)", Styles.BoldBlue));
        writer.Write(version);
        writer.WriteLine();
        writer.Write(tagLine2);
        writer.WriteLine();
        writer.WriteRule();
        writer.WriteLine();
    }
}
