namespace Ragnar.Tests;

public class CollectionInfoTests
{
    [Fact]
    public void CollectionInfoShouldStoreAllProperties()
    {
        var info = new Core.CollectionInfo
        {
            Name = "ragnar",
            Dimension = 768,
            VectorsCount = 1024,
            Status = "green"
        };

        Assert.Equal("ragnar", info.Name);
        Assert.Equal(768, info.Dimension);
        Assert.Equal(1024, info.VectorsCount);
        Assert.Equal("green", info.Status);
    }

    [Fact]
    public void CollectionInfoShouldSupportEquality()
    {
        var a = new Core.CollectionInfo { Name = "x", Dimension = 128, VectorsCount = 10, Status = "green" };
        var b = new Core.CollectionInfo { Name = "x", Dimension = 128, VectorsCount = 10, Status = "green" };

        Assert.Equal(a, b);
    }

    [Fact]
    public void CollectionInfoShouldDifferWhenDimensionDiffers()
    {
        var a = new Core.CollectionInfo { Name = "x", Dimension = 128, VectorsCount = 10, Status = "green" };
        var b = new Core.CollectionInfo { Name = "x", Dimension = 256, VectorsCount = 10, Status = "green" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void CollectionInfoShouldBeImmutable()
    {
        var info = new Core.CollectionInfo { Name = "immutable", Dimension = 64, VectorsCount = 5, Status = "yellow" };
        var copy = info with { Name = "changed" };

        Assert.Equal("immutable", info.Name);
        Assert.Equal("changed", copy.Name);
    }
}
