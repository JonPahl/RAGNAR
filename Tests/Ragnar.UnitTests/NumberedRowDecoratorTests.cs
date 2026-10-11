namespace Ragnar.Tests;

public class NumberedRowDecoratorTests
{
    private readonly Mock<ITableRenderer> _innerMock;
    private readonly ConsoleTableBuilder _table;

    public NumberedRowDecoratorTests()
    {
        _table = new ConsoleTableBuilder();
        _table.AddColumns("Name", "Value");

        KeyValuePair<int, List<TableCell>> row1 = new(0, [new TableCell("Alpha"), new TableCell("42")]);
        string[] cell1 = ["Alpha", "42"];

        KeyValuePair<int, List<TableCell>> row2 = new(0, [new TableCell("Beta"), new TableCell("43")]);
        string[] cell2 = ["Beta", "43"];

        _table.AddRow(row1, cell1);
        _table.AddRow(row2, cell2);

        _innerMock = new Mock<ITableRenderer>();
        _innerMock.SetupGet(m => m.Table).Returns(_table);
    }

    [Fact]
    public void TableShouldReturnInnerTable()
    {
        var sut = new NumberedRowDecorator(_innerMock.Object);
        Assert.Same(_table, sut.Table);
    }

    [Fact]
    public void RenderShouldAddIndexColumnIfMissing()
    {
        _table.Clear();
        var sut = new NumberedRowDecorator(_innerMock.Object, "#");
        sut.Render();
        Assert.Contains("#", _table.Columns, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderShouldDelegateToInnerRenderer()
    {
        _table.Clear();
        var sut = new NumberedRowDecorator(_innerMock.Object);
        sut.Render();
        _innerMock.Verify(m => m.Render(), Times.Once);
    }

    [Fact]
    public void RenderWithCustomHeaderShouldUseCustomColumn()
    {
        _table.Clear();
        var sut = new NumberedRowDecorator(_innerMock.Object, "Idx");
        sut.Render();
        Assert.Contains("Idx", _table.Columns, StringComparer.OrdinalIgnoreCase);
    }
}
