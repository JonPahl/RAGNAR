namespace Ragnar.Exceptions;

/// <summary>
/// Represents a non-recoverable error encountered during a pipeline stage execution.
/// </summary>
/// <example><![CDATA[throw new PipelineStageException("Stage failed.", ex);]]></example>
[Serializable]
public sealed class PipelineStageException : Exception
{
    /// <summary>Gets the name of the pipeline stage that caused this exception.</summary>
    public string StageName { get; }

    /// <summary>Initializes a new instance with the default system-supplied message.</summary>
    /// <example><![CDATA[throw new PipelineStageException();]]></example>
    public PipelineStageException()
        : base() // Use base() to initialize exception with default message
    {
    }

    /// <summary>Initializes a new instance with a custom error message.</summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <example><![CDATA[throw new PipelineStageException("Bad input.");]]></example>
    public PipelineStageException(string? message)
        : base(message) // Use base() to initialize exception with provided message
    {
    }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <param name="innerException">The original exception that caused this error.</param>
    /// <example><![CDATA[throw new PipelineStageException("Fail", ex);]]></example>
    public PipelineStageException(string? message, Exception? innerException)
        : base(message, innerException) // Use base() to initialize exception with provided message and inner exception
    {
        StageName = message;
    }
}