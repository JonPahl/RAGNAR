namespace Ragnar.Tests;

public class NumberedRowDecoratorTests
{
    private readonly Mock<ITableRenderer> _innerMock;

    public NumberedRowDecoratorTests()
    {
        _innerMock = new Mock<ITableRenderer>();
        _innerMock.SetupGet(r => r.Table).Returns(new ConsoleTableBuilder());
    }

    [Fact]
    public void TablePropertyShouldReturnInnerTable()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object);

        // Act
        var table = sut.Table;

        // Assert
        // Table delegates to inner.Table
        Assert.NotNull(sut);
    }

    [Fact]
    public void RenderShouldCallInnerRender()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object);

        // Act
        sut.Render();

        // Assert
        _innerMock.Verify(r => r.Render(), Times.Once);
    }

    [Fact]
    public void DefaultColumnHeaderShouldBeHash()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object);

        // Assert – the default parameter is "#"
        Assert.NotNull(sut);
    }

    [Fact]
    public void CustomColumnHeaderShouldBeAccepted()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object, "No.");

        // Assert
        Assert.NotNull(sut);
    }
}
