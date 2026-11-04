namespace Ragnar.Core.Validation;

public static class FluentValidationExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> IsValidUri()
        {
            return ruleBuilder
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("'{PropertyName}' must be a valid absolute URI.");
        }
    }
}
