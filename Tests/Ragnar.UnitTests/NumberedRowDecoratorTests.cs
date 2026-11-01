namespace Ragnar.Tests;

public class NumberedRowDecoratorTests
{
    private readonly Mock<ITableRenderer> _innerMock;
    private readonly Mock<ConsoleTableBuilder> _tableMock;

    public NumberedRowDecoratorTests()
    {
        _tableMock = new Mock<ConsoleTableBuilder>();
        _innerMock = new Mock<ITableRenderer>();
        _innerMock.Setup(i => i.Table).Returns(_tableMock.Object);
    }

    [Fact]
    public void TableShouldReturnInnerTable()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object);

        // Act
        var table = sut.Table;

        // Assert
        Assert.Same(_tableMock.Object, table);
    }

    [Fact]
    public void RenderShouldNotDoubleNumber()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object, "#");
        var columns = new List<string> { "#", "Name" };
        _tableMock.SetupGet(t => t.Columns).Returns(columns);
        _tableMock.Setup(t => t.Columns.Contains("#", StringComparer.OrdinalIgnoreCase)).Returns(true);

        // Act – call twice
        sut.Render();
        sut.Render();

        // Assert – inner Render called twice (no exception)
        _innerMock.Verify(i => i.Render(), Times.Exactly(2));
    }

    [Fact]
    public void RenderShouldDelegateToInnerRenderer()
    {
        // Arrange
        var sut = new NumberedRowDecorator(_innerMock.Object);
        var columns = new List<string> { "#", "Col" };
        _tableMock.SetupGet(t => t.Columns).Returns(columns);
        _tableMock.Setup(t => t.Columns.Contains("#", StringComparer.OrdinalIgnoreCase)).Returns(true);

        // Act
        sut.Render();

        // Assert
        _innerMock.Verify(i => i.Render(), Times.Once);
    }
}
