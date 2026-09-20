// ═══════════════════════════════════════════════════════════
// VectorStoreRepositoryTests.cs
// ═══════════════════════════════════════════════════════════
namespace Ragnar.Tests;

public class TableBuilderTests
{
    private readonly ConsoleTableBuilder _sut = new();

    public TableBuilderTests()
    {
        _sut = new();
    }

    [Fact]
    public void AddColumnsShouldReturnSameInstance()
    {
        // Act
        var result = _sut.AddColumns("A", "B", "C");

        // Assert
        Assert.Same(_sut, result);
    }

    [Fact]
    public void AddColumnsShouldAddAllColumns()
    {
        // Act
        _sut.AddColumns("Name", "Age", "City");

        // Assert
        Assert.Equal(["Name", "Age", "City"], _sut.Columns);
    }

    [Fact]
    public void AddRowShouldReturnSameInstance()
    {
        // Act
        var result = _sut.AddRow("val1", "val2");

        // Assert
        Assert.Same(_sut, result);
    }

    [Fact]
    public void AddRowShouldAddRowToRowsCollection()
    {
        // Act
        _sut.AddRow("a", "b");
        _sut.AddRow("c", "d");

        // Assert
        Assert.Equal(2, _sut.Rows.Count);
        Assert.Equal(new[] { "a", "b" }, _sut.Rows[0]);
        Assert.Equal(new[] { "c", "d" }, _sut.Rows[1]);
    }

    [Fact]
    public void ToTableShouldReturnTableWithColumnsAndRows()
    {
        // Arrange
        _sut.AddColumns("Col1", "Col2");
        _sut.AddRow("v1", "v2");
        _sut.AddRow("v3", "v4");

        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.NotNull(table);
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal(2, table.Rows.Count);
    }

    [Fact]
    public void ToTableEmptyBuilderShouldReturnEmptyTable()
    {
        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.NotNull(table);
        Assert.Empty(table.Columns);
        Assert.Empty(table.Rows);
    }

    [Fact]
    public void ShowRowSeparatorsShouldDefaultToFalse()
    {
        Assert.False(_sut.ShowRowSeparators);
    }

    [Fact]
    public void ExpandShouldDefaultToFalse()
    {
        Assert.False(_sut.Expand);
    }

    [Fact]
    public void ChainingAddColumnsThenRowsShouldWork()
    {
        // Act
        _sut.AddColumns("X").AddRow("1").AddRow("2");

        // Assert
        Assert.Single(_sut.Columns);
        Assert.Equal(2, _sut.Rows.Count);
    }
}
