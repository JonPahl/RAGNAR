namespace Ragnar;
/// <summary>
/// Represents a non-recoverable error encountered during a pipeline stage execution.
/// </summary>
/// <example><![CDATA[throw new PipelineStageException("Stage failed.", ex);]]></example>
[Serializable]
public sealed class PipelineStageException : Exception
{
    public IAsyncEnumerable<char>? StageName;

    /// <summary>Initializes a new instance with the default system-supplied message.</summary>
    /// <example><![CDATA[throw new PipelineStageException();]]></example>
    public PipelineStageException()
    {
    }

    /// <summary>Initializes a new instance with a custom error message.</summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <example><![CDATA[throw new PipelineStageException("Bad input.");]]>
    /// </example>
    public PipelineStageException(string? message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <param name="innerException">The original exception that caused this error.</param>
    /// <example><![CDATA[throw new PipelineStageException("Fail", ex);]]></example>
    public PipelineStageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
