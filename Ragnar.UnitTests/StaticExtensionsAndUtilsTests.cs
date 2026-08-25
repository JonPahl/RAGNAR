namespace Ragnar.Tests;

public class StaticExtensionsAndUtilsTests
{
    [Fact]
    public void ElapsedTimeString_FormatsAsMM_SS()
    {
        var sw = Stopwatch.StartNew();
        Thread.Sleep(10); // Ensure elapsed time > 0
        var formatted = sw.ElapsedTimeString();

        formatted.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }

    [Fact]
    public void ExpandDirectory_Throws_When_Path_Does_Not_Exist()
    {
        var nonExistentPath = @"C:\" + Guid.NewGuid().ToString();
        Assert.Throws<DirectoryNotFoundException>(() => nonExistentPath.ExpandDirectory());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExpandDirectory_Throws_For_NullOrEmpty(string? Path)
    {
        Assert.ThrowsAny<Exception>(() => Path!.ExpandDirectory());
    }

    [Fact]
    public void GetStyle_ReturnsPlain_When_Style_Is_Null()
    {
        Style? nullStyle = null;
        var result = nullStyle.GetStyle;

        Assert.Equal(Style.Plain, result);
    }

    //[Fact]
    //public void InformationalVersion_ReturnsFallback_When_Attribute_Missing()
    //{
    //    var asmMock = new Mock<IAssemblyInfo>();
    //    asmMock.Setup(x => x.InformationalVersion).Returns((string?)null);

    //    var version = asmMock.Object.InformationalVersion;
    //    Assert.Equal("1.0.0", version);
    //}
}

