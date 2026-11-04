namespace Ragnar.Embedding;

/// <summary>Extends <see cref="string"/> with utility methods for comment processing.</summary>
public static class StringExtensions
{
    /// <summary>Provides utility extension methods on string values.</summary>
    /// <example><![CDATA[bool ok = "file.cs".IsExcluded(exclList);]]></example>
    extension(string value)
    {
        /// <summary>
        /// Counts non-tag, non-comment characters in an XML comment.
        /// </summary>
        /// <returns>The count of non-tag, non-comment characters.</returns>
        /// <example>
        /// <![CDATA[var count="<c>code</c>".CharacterCount();]]>
        /// </example>
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == '<') inTag = true;
                else if (value[i] == '>') inTag = false;
                else if (!inTag && value[i] != '/')
                    count++;
            }
            return count;
        }

        /// <summary>Retrieves the last folder segment from a file-system path.</summary>
        /// <returns>The last folder name extracted from the path.</returns>
        /// <example><![CDATA[var f = @"C:\src\proj\app".FolderName(); // "app"]]></example>
        public string FolderName => Path.GetFileName(value.TrimEnd('/', '\\'));
    }
}
