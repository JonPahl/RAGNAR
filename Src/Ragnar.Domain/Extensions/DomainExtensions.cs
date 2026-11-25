namespace Ragnar.Domain.Extensions;

public static class DomainExtensions
{

    // Directly extends domain models without static wrapper classes
    extension(Question q)
    {
        public Question SetFilter(Filter? filter) =>
            filter is not null ? new(q.IsEnabled, q.Text, q.Filename, q.Category, filter) : q;

        public Question ValidateQuestion()
        {
            var result = new QuestionValidator().Validate(q);
            if (result.IsValid) return q;

            var errors = string.Join("\n", result.Errors.Select(static e => $"{e.PropertyName}: {e.ErrorMessage}"));
            throw new ValidationException(errors);
        }
    }

    extension(IReadOnlyList<Question> questions)
    {
        public IReadOnlyList<Question> ActiveOnly => [.. questions.Where(static q => q.IsEnabled)];
        public IReadOnlyList<Question> InActiveOnly => [.. questions.Where(static q => !q.IsEnabled)];

        public IReadOnlyList<Question> WithCategory(HashSet<QuestionCategory> categories)
        {
            if (categories?.Count == 0)
            {
                return [.. questions];
            }
            else
            {
                var items = questions.SelectMany(question => categories!
                .Where(cat => question.Category.Equals(cat))
                .Select(_ => question)).ToList();
                return items.AsReadOnly();
            }
        }
    }

    extension(CodeDocument doc)
    {
        public IDictionary<string, Value> Dictionary => new Dictionary<string, Value>
    {
        { nameof(doc.FileName), doc.FileName.Replace("//", "/", StringComparison.InvariantCultureIgnoreCase) },
        { nameof(doc.ElementType), doc.ElementType ?? string.Empty },
        { nameof(doc.ElementName), doc.ElementName ?? string.Empty },
        { nameof(doc.Comment), doc.Comment ?? string.Empty },
        { nameof(doc.CommentLength), (Value)doc.CommentLength },
        { nameof(doc.Code), doc.Code ?? string.Empty },
        { nameof(doc.Category), (Value)(doc.Category ?? nameof(QuestionCategory.General)) },
        { nameof(doc.ContentHash), doc.ContentHash ?? string.Empty }
    };
    }

    extension(string value)
    {
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == '<') inTag = true;
                else if (value[i] == '>') inTag = false;
                else if (!inTag && value[i] != '/') count++;
            }
            return count;
        }

        public string FolderName => Path.GetFileName(value.TrimEnd('/', '\\'));
    }

    extension(Style? style)
    {
        public Style EnsureValidStyle() => style ?? Style.Plain;
    }

    extension(Stopwatch sw)
    {
        public string FormatElapsedTime()
        {
            Guard.Against.Null(sw);
            var t = sw.Elapsed;
            return t.TotalHours >= 1
                ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}"
                : $"{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }

    extension(string value)
    {
        public bool IsExcluded(in IReadOnlyCollection<string> exclusions) => exclusions.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

}
