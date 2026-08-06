namespace RAGNAR.UnitTests;

public class QuestionValidatorTests
{
    [Fact]
    public void Validate_ShouldFail_WhenTextEmpty()
    {
        var validator = new QuestionValidator();
        var result = validator.Validate(new Question(true, "", "f.cs", QuestionCategory.XML));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Question.Text));
    }
}
