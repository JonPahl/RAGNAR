namespace Ragnar.Validator;

public class ApplicationOptionsValidation
    : AbstractValidator<ApplicationOptions>
{
    public ApplicationOptionsValidation()
    {
        RuleFor(x => x.VectorStoreName).NotEmpty().WithMessage("Qdrant Vector Store Name is required.");
        RuleFor(x => x.SourceDirectory).Must(Directory.Exists).WithMessage("Source directory must exists.");
    }
}

