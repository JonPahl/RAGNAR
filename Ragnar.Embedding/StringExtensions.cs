using System.Text.RegularExpressions;

namespace Ragnar.Embedding;

public static class StringExtensions
{
    extension(string xmlComment)
    {
        /// <summary>Counts non-tag, non-comment characters in XML comment.</summary>
        /// <returns>Character count excluding XML tags and slashes.</returns>
        /// <example><![CDATA[int len = comment.CharacterCount();]]></example>
        public int CharacterCount()
        {
            string noTags = Regex.Replace(xmlComment, "<.*?>", string.Empty);

            string cleanText = noTags.Replace("/", "").Trim();

            return cleanText.Length;
        }
    }
}