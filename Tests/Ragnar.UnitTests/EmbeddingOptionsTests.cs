namespace Ragnar.Tests;

public class EmbeddingOptionsTests
{
    private EmbeddingOptions ValidOptions() => new()
    {
        Host = new Uri("http://localhost:11434"),
        EmbeddingModel = "nomic-embed-text",
        Timeout = TimeSpan.FromSeconds(60),
        Dimension = 768,
        BatchSize = 16
    };

    //    [Fact]
    //    public void ValidOptionsShouldPassValidation()
    //    {
    //        // Act
    //        var results = new List<ValidationResult>();
    //        var valid = TryValidateObject(ValidOptions(), new ValidationContext(ValidOptions()), results);

    //        // Assert
    //        Assert.True(valid, string.Join("; ", results.SelectMany(r => r.ErrorMessage ?? r.MemberNames!.ToArray())));
    //    }

    //    [Fact]
    //    public void EmbeddingModelTooShortShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { EmbeddingModel = "a" }; // 1 char < 2

    //        var results = new List<ValidationResult>();
    //        var valid = Ragnar.Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("EmbeddingModel")));
    //    }

    //    [Fact]
    //    public void EmbeddingModelTooLongShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { EmbeddingModel = new string('x', 257) };

    //        var results = new List<ValidationResult>();
    //        var valid = Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("EmbeddingModel")));
    //    }

    //    [Fact]
    //    public void TimeoutBelowOneSecondShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { Timeout = TimeSpan.Zero };

    //        var results = new List<ValidationResult>();
    //        var valid = Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("Timeout")));
    //    }

    //    [Fact]
    //    public void DimensionBelowOneShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { Dimension = 0 };

    //        var results = new List<ValidationResult>();
    //        var valid = Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("Dimension")));
    //    }

    //    [Fact]
    //    public void BatchSizeBelowOneShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { BatchSize = 0 };

    //        var results = new List<ValidationResult>();
    //        var valid = Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("BatchSize")));
    //    }

    //    [Fact]
    //    public void BatchSizeAbove512ShouldFailValidation()
    //    {
    //        var opts = ValidOptions() with { BatchSize = 513 };

    //        var results = new List<ValidationResult>();
    //        var valid = Validator.TryValidateObject(opts, new ValidationContext(opts), results);

    //        Assert.False(valid);
    //        Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("BatchSize")));
    //    }

    //    [Fact]
    //    public void RecordEqualityShouldWorkForIdenticalOptions()
    //    {
    //        var a = ValidOptions();
    //        var b = ValidOptions();

    //        Assert.Equal(a, b);
    //    }

    //    [Fact]
    //    public void RecordEqualityShouldFailWhenDimensionDiffers()
    //    {
    //        var a = ValidOptions();
    //        var b = ValidOptions() with { Dimension = 1024 };

    //        Assert.NotEqual(a, b);
    //    }
}
