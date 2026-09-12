namespace Ragnar.Extensions;

/// <summary>Extends <see cref="string"/> with utility methods for comment processing.</summary>
public static class StringExtensions
{
    extension(ReadOnlySpan<char> value)
    {
        /// <summary>
        /// Counts non-tag, non-comment characters in an XML comment.
        /// </summary>
        /// <returns>The count of non-tag, non-comment characters.</returns>
        /// <example><![CDATA[var count = "<c>code</c>".CharacterCount();]]></example>
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
        /// <example><![CDATA[var f = @"C:\src\proj\app".LastFolder(); // "app"]]></example>
        public string LastFolder => Path.GetFileName(value.ToString().TrimEnd('/', '\\'));


        /// <summary>Checks whether the filename appears in the exclusion list.</summary>
        /// <param name="exclusions">Set of filenames to exclude (case-insensitive).</param>
        /// <returns><c>true</c> if the filename is excluded; otherwise <c>false</c>.</returns>
        /// <example><![CDATA[bool ok = "docker-compose.yml".IsExcluded(exclList);]]></example>
        public bool IsExcluded(in IReadOnlyCollection<string> exclusions)
            => exclusions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
