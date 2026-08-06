namespace Ragnar.Extensions;

public static class ClientExtensions
{
    /// <summary>Validates and normalizes host URI.
    /// </summary>
    /// <param name="Host">Host string.</param>
    /// <returns>Validated host URI.</returns>
    /// <example><![CDATA[var host = factory.ValidateHost("localhost");]]></example>
    public static string ValidateHost(this string Host)
    {
        Guard.Against.NullOrEmpty(Host);

        if(!Host.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            Host = $"http://{Host}";
        }

        if(!Uri.TryCreate(Host, UriKind.RelativeOrAbsolute, out var ValidUri))
            throw new ArgumentException("Cannot create uri from provided host");

        return ValidUri.OriginalString;
    }

    /// <summary>Validates port is in TCP range 1–65535.</summary>
    /// <param name="Port">Port number.</param>
    /// <returns>Valid port.</returns>
    /// <example><![CDATA[var port = factory.ValidatePort(8080);]]></example>
    public static int ValidatePort(this int Port)
    {
        if(Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(Port), "Port must be between 1 and 65535.");
        }

        return Port;
    }
}
