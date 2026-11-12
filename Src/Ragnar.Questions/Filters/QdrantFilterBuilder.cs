namespace Ragnar.Questions.Filters;

/// <summary>Fluent builder for constructing Qdrant filter expressions.</summary>
/// <example><![CDATA[var f = new QdrantFilterBuilder().AddRange("len", 50).Build();]]></example>
public sealed class QdrantFilterBuilder
{
    /// <summary>Positive (must-match) filter conditions.</summary>
    private readonly List<Condition> _must = [];

    /// <summary>Negative (must-not-match) filter conditions.</summary>
    private readonly List<Condition> _mustNot = [];

    private readonly List<Condition> _should = [];

    private static void GuardField(string field) =>
    Guard.Against.NullOrWhiteSpace(field);

    /// <summary>Adds a keyword match-exclusion condition.</summary>
    /// <param name="field">The payload field to match.</param>
    /// <param name="value">The keyword value to exclude.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.AddMustNotMatch("category", "skip");]]></example>
    public QdrantFilterBuilder AddMustNotMatch(string field, string value)
    {
        GuardField(field);
        Guard.Against.Null(value);
        _mustNot.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    /// <summary>Adds a keyword match-inclusion condition to the positive filter set.</summary>
    /// <param name="field">The payload field to match against.</param>
    /// <param name="value">The keyword value that must be present.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[[builder.AddMustMatch("Category", "XML");]]></example>
    public QdrantFilterBuilder AddMustMatch(string field, string value)
    {
        GuardField(field);
        Guard.Against.Null(value);
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
        GuardField(field);

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
    /// <example><![CDATA[builder.AddMustNotRange("CommentLength", null, 500);]]></example>
    public QdrantFilterBuilder AddMustNotRange(string field, double? gte = null, double? lte = null)
    {
        GuardField(field);

        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.");

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;

        _mustNot.Add(new Condition { Field = new FieldCondition { Key = field, Range = range } });

        return this;
    }

    /// <summary>Adds a keyword match condition to the positive "should" filter set.</summary>
    /// <param name="field">The payload field to match against.</param>
    /// <param name="value">The keyword value that may be present.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddShouldMatch("Category", "XML");]]></example>
    public QdrantFilterBuilder AddShouldMatch(string field, string value)
    {
        GuardField(field);
        _should.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    /// <summary>Adds an is-null condition requiring the field to be absent.</summary>
    /// <param name="field">The payload field expected to be null.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddIsNull("CommentText");]]></example>
    public QdrantFilterBuilder AddIsNull(string field)
    {
        GuardField(field);
        _must.Add(new Condition { IsNull = new IsNullCondition { Key = field } });
        return this;
    }

    /// <summary>Adds an is-not-null condition requiring the field to be present.</summary>
    /// <param name="field">The payload field expected to have a value.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddIsNotNull("SourceFile");]]></example>
    public QdrantFilterBuilder AddIsNotNull(string field)
    {
        GuardField(field);

        _mustNot.Add(new Condition
        {
            IsNull = new IsNullCondition { Key = field }
        });

        return this;
    }

    /// <summary>Adds an except-keywords condition excluding the given values from a field.</summary>
    /// <param name="field">The payload field containing keywords.</param>
    /// <param name="keywords">Enumerable of keyword values to exclude.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddExceptKeywords("Category", new[]{"Test","Tests"});]]></example>
    public QdrantFilterBuilder AddExceptKeywords(string field, IEnumerable<string> keywords)
    {
        GuardField(field);
        var list = keywords as IReadOnlyCollection<string> ?? [.. keywords];
        if (list.Count == 0) return this;
        _must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match { ExceptKeywords = new RepeatedStrings { Strings = { list } } }
            }
        });
        return this;
    }

    /// <summary>Removes all accumulated must, mustNot, and should filter conditions.</summary>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.Clear();]]></example>
    public QdrantFilterBuilder Clear()
    {
        _must.Clear();
        _mustNot.Clear();
        _should.Clear();
        return this;
    }

    /// <summary>Indicates whether no filter conditions have been added yet.</summary>
    /// <example><![CDATA[bool empty = builder.IsEmpty;]]></example>
    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0 && _should.Count == 0;

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
            MustNot = { _mustNot },
            Should = { _should }
        };
    }

    /// <summary>Non-throwing variant; returns <c>false</c> if the builder is empty.</summary>
    /// <param name="filter">The composed filter, or null if the builder is empty.</param>
    /// <returns><c>true</c> if a filter was produced; <c>false</c> if the builder is empty.</returns>
    /// <example><![CDATA[if (builder.TryBuild(out var f)) { use f; }]]></example>
    public bool TryBuild(out Filter filter)
    {
        if (IsEmpty) { filter = null!; return false; }
        filter = Build();
        return true;
    }

    public override string ToString() =>
        $"QdrantFilterBuilder[must:{_must.Count} mustNot:{_mustNot.Count} should:{_should.Count}]";
}
