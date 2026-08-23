namespace Ragnar.Extensions;

/// <summary> Provides extension methods for <see cref="FileSystemEntry"/>.</summary>
public static class FileSystemEntryExtensions
{
    /// <summary>
    /// Determines whether the file has one of the allowed extensions (case-insensitive).
    /// </summary>
    /// <param name="Entry">The file system entry.</param>
    /// <param name="AllowedExtensions">The collection of allowed file extensions.</param>
    /// <returns><c>true</c> if the extension is allowed; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <![CDATA[
    /// var entry = new FileSystemEntry(new FileInfo("document.pdf"));
    /// bool isAllowed = entry.HasAllowedExtension([".pdf", ".docx"]);
    /// ]]>
    /// </example>
    public static bool HasAllowedExtension(
        this FileSystemEntry Entry,
        IReadOnlyCollection<string> AllowedExtensions)
        => !Entry.IsDirectory && AllowedExtensions.Contains(Path.GetExtension(Entry.FileName.ToString()), StringComparer.OrdinalIgnoreCase);
}
