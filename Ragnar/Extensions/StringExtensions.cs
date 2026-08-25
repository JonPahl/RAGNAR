namespace Ragnar.Extensions;

/// <summary>Extends <see cref="string"/> with utility methods for comment processing.</summary>
public static class StringExtensions
{
    extension(ReadOnlySpan<char> Value)
    {
        /// <summary>
        /// Counts non-tag, non-comment characters in an XML comment.
        /// </summary>
        /// <returns>The count of non-tag, non-comment characters.</returns>
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;

            for (var i = 0; i < Value.Length; i++)
            {
                if (Value[i] == '<') inTag = true;
                else if (Value[i] == '>') inTag = false;
                else if (!inTag && Value[i] != '/')
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Retrieves the last folder from a given path.
        /// </summary>
        /// <returns>The span of characters representing the last folder in the path.</returns>
        public string LastFolder => Path.GetFileName(Value.ToString().TrimEnd('/', '\\'));


        /// <summary>Checks if a filename is in the exclusion list (case-insensitive).</summary>
        /// <param name="FileName">Filename to check.</param>
        /// <param name="Exclusions">Set of excluded filenames.</param>
        /// <returns><c>true</c> if excluded; otherwise <c>false</c>.</returns>
        public bool IsExcluded(in IReadOnlyCollection<string> Exclusions)
            => Exclusions.Contains(Value.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
