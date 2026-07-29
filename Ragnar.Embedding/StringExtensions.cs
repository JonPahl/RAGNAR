namespace Ragnar.Embedding;

public static class StringExtensions
{
    extension(ReadOnlySpan<char> xmlComment)
    {
        /// <summary>Counts non-tag, non-comment characters in XML comment.</summary>
        /// <returns>Character count excluding XML tags and slashes.</returns>
        /// <example><![CDATA[int len = comment.CharacterCount();]]></example>
        public int CharacterCount ()
        {
            var count = 0;
            var inTag = false;

            foreach (var c in xmlComment)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (inTag || c == '/') continue;
                count++;
            }

            return count;
        }
    }
}
