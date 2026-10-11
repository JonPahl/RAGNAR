namespace Ragnar.Questions.Filters;

/// <summary>Fluent builder for constructing Qdrant filter expressions.</summary>
/// <example><![CDATA[var f = new FilterBuilder().AddRange("len", 50).Build();]]></example>
public sealed class FilterBuilder
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
    public FilterBuilder AddExclusion(string field, string value)
    {
        _mustNot.Add(Conditions.MatchKeyword(field, value));

        return this;
    }

    /// <summary>Adds a keyword match-inclusion condition to the positive filter set.</summary>
    /// <param name="field">The payload field to match against.</param>
    /// <param name="value">The keyword value that must be present.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[[builder.AddMatch("Category", "XML");]]></example>
    public FilterBuilder AddMatch(string field, string value)
    {
        _must.Add(Conditions.MatchKeyword(field, value));

        return this;
    }

    /// <summary>Adds a range condition (less-than) on a numeric field.</summary>
    /// <param name="field">The payload field name.</param>
    /// <param name="lt">The less-than upper bound.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.AddRange("length", 100);]]></example>
    /// <summary>Adds a range condition with lower and/or upper bounds.</summary>
    /// <param name="field">Payload field name.</param>
    /// <param name="gte">Greater-than-or-equal bound (optional).</param>
    /// <param name="lte">Less-than-or-equal bound (optional).</param>
    public FilterBuilder AddRange(string field, double? gte = null, double? lte = null)
    {
        var range = new Range();

        if (gte is not null)
        {
            range.Gte = gte.Value;
        }
        else if (lte is not null)
        {
            range.Lte = lte.Value;
        }
        else
        {
            throw new ArgumentException("At least one of gte or lte must be provided.");
        }

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
    public FilterBuilder AddExceptKeywords(string field, params string[] keywords)
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

    /// <summary>Excludes a range of values (must-not).</summary>
    public FilterBuilder AddExclusionRange(string field, double? gte = null, double? lte = null)
    {
        _mustNot.Add(new Condition
        {
            Field = new FieldCondition { Key = field, Range = new Range { Gte = gte.Value, Lte = lte.Value } }
        });
        return this;
    }

    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0;

    /// <summary>Builds and returns the final Qdrant <see cref="Filter"/> instance.</summary>
    /// <returns>A composed filter with must and mustNot conditions.</returns>
    /// <example><![CDATA[Filter f = builder.Build();]]></example>
    public Filter Build()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(
                "FilterBuilder is empty. Add at least one condition before calling Build().");
        }

        return new Filter
        {
            Must = { _must },
            MustNot = { _mustNot }
        };
    }
}
