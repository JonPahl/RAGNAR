namespace Ragnar.Output;


/// <summary>Interface for formatting Markdown.</summary>
public interface IOutputFormatter
{
    /// <summary>Gets or sets the file extension used by this formatter.</summary>
    string FileExtension { get; set; }

    ///<summary>Formats Markdown based on provided details.</summary>
    ///<param name ="details">Save details to use for formatting.</param>
    string Format(SaveDetails details);
}
