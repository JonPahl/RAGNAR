namespace Ragnar.Embedding;

public static class StringExtensions
{
    extension(string XmlComment)
    {
        /// <summary>Counts non-tag, non-comment characters in XML comment.</summary>
        /// <returns>Character count excluding XML tags and slashes.</returns>
        /// <example><![CDATA[int len = comment.CharacterCount();]]></example>
        public int CharacterCount()
        {
            var count = 0;
            var insideTag = false;

            for (var i = 0; i < XmlComment.Length; i++)
            {
                var c = XmlComment[i];

                if (c == '<') { insideTag = true; continue; }
                if (c == '>') { insideTag = false; continue; }

                if (!insideTag && c != '/' && !char.IsWhiteSpace(c))
                    count++;
            }

            return count;
        }
    }
}
