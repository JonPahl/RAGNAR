namespace Ragnar;

[Serializable]
internal class PipelineStageException : Exception
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