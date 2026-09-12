namespace Ragnar.Contracts;

/// <summary>Interface for displaying application branding and version information.</summary>
/// <example><![CDATA[header.RenderBranding();]]></example>
public interface IApplicationHeader
{
    /// <summary>Renders the Ragnar banner, tagline, and version to the console.</summary>
    /// <example><![CDATA[header.RenderBranding();]]></example>
    void RenderBranding();
}
