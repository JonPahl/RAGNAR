namespace Ragnar.Questions.Filters;

/// <summary>Fluent builder for constructing Qdrant filter expressions.</summary>
/// <example><![CDATA[var f = new QdrantFilterBuilder().AddRange("len", 50).Build();]]></example>
public sealed class QdrantFilterBuilder
{
    /// <summary>Positive (must-match) filter conditions.</summary>
    private readonly List<Condition> _must = [];

    /// <summary>Negative (must-not-match) filter conditions.</summary>
    private readonly List<Condition> _mustNot = [];

    /// <summary>Adds a keyword match-exclusion condition.</summary>
    /// <param name="field">The payload field to match.</param>
    /// <param name="value">The keyword value to exclude.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.AddExclusion("category", "skip");]]></example>
    public QdrantFilterBuilder AddExclusion(string field, string value)
    {
        _mustNot.Add(Conditions.MatchKeyword(field, value));

        return this;
    }

    /// <summary>Adds a keyword match-inclusion condition to the positive filter set.</summary>
    /// <param name="field">The payload field to match against.</param>
    /// <param name="value">The keyword value that must be present.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[[builder.AddMatch("Category", "XML");]]></example>
    public QdrantFilterBuilder AddMatch(string field, string value)
    {
        _must.Add(Conditions.MatchKeyword(field, value));

        return this;
    }

    /// <summary>Adds a numeric range condition (gte and/or lte) to the positive filter set.</summary>
    /// <param name="field">The payload field name to constrain.</param>
    /// <param name="gte">Greater-than-or-equal lower bound (optional).</param>
    /// <param name="lte">Less-than-or-equal upper bound (optional).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddRange("CommentLength", 50, 500);]]></example>
    public QdrantFilterBuilder AddRange(string field, double? gte = null, double? lte = null)
    {
        var range = new Range();

        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.");

        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;

        _must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Range = range
            }
        });

        return this;
    }

    /// <summary>Adds an except-keywords condition excluding specified values from a field.</summary>
    /// <param name="field">The payload field containing keywords.</param>
    /// <param name="keywords">Keyword values to exclude from matches.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddExceptKeywords("Category", "Test", "Tests");]]></example>
    public QdrantFilterBuilder AddExceptKeywords(string field, params string[] keywords)
    {
        _must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match
                {
                    ExceptKeywords = new RepeatedStrings
                    {
                        Strings = { keywords }
                    }
                }
            }
        });

        return this;
    }

    /// <summary>Adds a numeric range must-not condition to the negative filter set.</summary>
    /// <param name="field">The payload field name to constrain.</param>
    /// <param name="gte">Greater-than-or-equal bound (optional).</param>
    /// <param name="lte">Less-than-or-equal bound (optional).</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public QdrantFilterBuilder AddExclusionRange(string field, double? gte = null, double? lte = null)
    {
        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.");

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;

        _mustNot.Add(new Condition { Field = new FieldCondition { Key = field, Range = range } });

        return this;
    }

    /// <summary>Indicates whether no filter conditions have been added yet.</summary>
    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0;

    /// <summary>Builds and returns the final Qdrant <see cref="Filter"/> instance.</summary>
    /// <returns>A composed filter with must and mustNot conditions.</returns>
    /// <example><![CDATA[Filter f = builder.Build();]]></example>
    public Filter Build()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(
                "QdrantFilterBuilder is empty. Add at least one condition before calling Build().");
        }

        return new Filter
        {
            Must = { _must },
            MustNot = { _mustNot }
        };
    }
}
