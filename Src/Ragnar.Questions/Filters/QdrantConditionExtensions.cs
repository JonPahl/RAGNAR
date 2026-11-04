namespace Ragnar.Questions.Filters;

public static class QdrantConditionExtensions
{
    public static Condition MatchKeyword(this Condition _, string field, string value) =>
        Conditions.MatchKeyword(field, value);

    public static Condition Range(this Condition _, string field, double? gte = null, double? lte = null) =>
        new() { Field = new FieldCondition { Key = field, Range = BuildRange(gte, lte) } };

    public static Condition ExceptKeywords(this Condition _, string field, string[] keywords) =>
        new()
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match { ExceptKeywords = new RepeatedStrings { Strings = { keywords } } }
            }
        };

    private static Range BuildRange(double? gte, double? lte)
    {
        ArgumentNullException.ThrowIfNull(gte is null && lte is null);
        var r = new Range();
        if (gte is not null) r.Gte = gte.Value;
        if (lte is not null) r.Lte = lte.Value;
        return r;
    }

}
