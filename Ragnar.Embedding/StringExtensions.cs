namespace Ragnar.Embedding;

public static class StringExtensions
{

    /// <summary>Extension methods for counting characters in XML comment strings.</summary>
    /// <example>
    /// <![CDATA[var c = "<c>code</c>".CharacterCount();]]>
    /// </example>
    extension(string xmlComment)
    {
        /// <summary>Counts non-tag, non-comment characters in an XML comment.</summary>
        /// <returns>The count of non-tag, non-comment characters.</returns>
        /// <example>
        /// <![CDATA[var count = "<c>code</c>".CharacterCount();]]>
        /// </example>
        public int CharacterCount()
        {
            var count = 0; var insideTag = false;
            for (var i = 0; i < xmlComment.Length; i++)
            {
                var c = xmlComment[i];
                if (c == '<') { insideTag = true; continue; }
                if (c == '>') { insideTag = false; continue; }
                if (!insideTag && c != '/') count++;
            }
            return count;
        }
    }
}
