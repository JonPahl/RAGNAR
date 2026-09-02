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

        /// <summary>
        /// Retrieves the last folder from a given path.
        /// </summary>
        /// <returns>The span of characters representing the last folder in the path.</returns>
        public string LastFolder => Path.GetFileName(value.ToString().TrimEnd('/', '\\'));


        /// <summary>Checks if a filename is in the exclusion list (case-insensitive).</summary>
        /// <param name="exclusions">Set of excluded filenames.</param>
        /// <returns><c>true</c> if excluded; otherwise <c>false</c>.</returns>
        public bool IsExcluded(in IReadOnlyCollection<string> exclusions)
            => exclusions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
