namespace Ragnar;

[Serializable]
public sealed class PipelineStageException : Exception
{
    public PipelineStageException()
    {
    }

    public PipelineStageException(string? message) : base(message)
    {
    }

    public PipelineStageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
