namespace Ragnar.Embedding;

public static class StringExtensions
{
    extension(ReadOnlySpan<char> XmlComment)
    {
        /// <summary>Counts non-tag, non-comment characters in XML comment.</summary>
        /// <returns>Character count excluding XML tags and slashes.</returns>
        /// <example><![CDATA[int len = comment.CharacterCount();]]></example>
        public int CharacterCount()
        {
            var Count = 0;
            var InTag = false;

            foreach(var Comment in XmlComment)
            {
                if(Comment == '<') { InTag = true; continue; }
                if(Comment == '>') { InTag = false; continue; }
                if(InTag || Comment == '/') continue;
                Count++;
            }

            return Count;
        }

    }

    public static ReadOnlySpan<char> GetLastFolder(this string Path)
    {
        if(string.IsNullOrEmpty(Path)) return ReadOnlySpan<char>.Empty;

        // Convert to span and trim any trailing slashes
        ReadOnlySpan<char> PathSpan = Path.AsSpan().TrimEnd(System.IO.Path.DirectorySeparatorChar);

        // Find the last separator remaining
        var LastSlash = PathSpan.LastIndexOf(System.IO.Path.DirectorySeparatorChar);

        // Slice out and return just the final folder name
        return LastSlash < 0 ? PathSpan : PathSpan.Slice(LastSlash + 1);
    }

}
