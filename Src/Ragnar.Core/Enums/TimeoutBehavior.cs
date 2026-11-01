namespace Ragnar.Core.Enums;

/// <summary>
/// Specifies how to handle timeouts in pipeline stages.
/// </summary>
public enum TimeoutBehavior
{
    /// <summary>
    /// Throw an exception when a timeout occurs.
    /// </summary>
    Throw,

    /// <summary>
    /// Skip the current stage and continue execution.
    /// </summary>
    Skip,

    /// <summary>
    /// Return the pipeline context without throwing or skipping.
    /// </summary>
    ReturnContext
}
