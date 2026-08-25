namespace Ragnar.Tests.Extensions;

public class SavePathExtensionsTests
{


    [Theory]
    [InlineData("", "Response")]
    [InlineData("/app/data", @"/app/data\Response")]
    public void GetResponseDirectory_StringOverload_HandlesBasePath(string BaseDir, string Expected)
    {
        var result = BaseDir.GetResponseDirectory();
        result.Should().Be(Expected);
    }
}
