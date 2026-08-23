namespace Ragnar.Contracts;

/// <summary>
/// Displays branding information.
/// </summary>
public interface IApplicationHeader
{
    /// <summary>
    /// Displays branding banner to console.
    /// </summary>
    /// <example><![CDATA[branding.RenderBranding();]]></example>
    void RenderBranding();
}
