using Ragnar.Core.Rendering;

namespace Ragnar.Tests;

public sealed class ConsoleTableBuilderTests
{
    private readonly ConsoleTableBuilder _sut;
    public ConsoleTableBuilderTests() => _sut = new();

    [Fact]
    public void ClearShouldResetState()
    {
        // Arrange
        _sut.AddColumns("A").AddRow(new KeyValuePair<int, List<TableCell>>(0, []), "1");
        Assert.True(_sut.Columns.Count > 0);

        // Act
        var result = _sut.Clear();

        // Assert
        Assert.Same(_sut, result);
        Assert.Empty(_sut.Columns);
        Assert.Empty(_sut.Rows);
    }

    [Fact]
    public void AddRowShouldThrowOnNullCells()
    {
        // Arrange
        var row = new KeyValuePair<int, List<TableCell>>(0, []);

        // Act & Assert
        var ex = Assert.ThrowsAny<ArgumentException>(() => _sut.AddRow(row, null!));
        Assert.True(ex.Message.Contains("Empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddRowShouldThrowOnCellCountMismatch()
    {
        // Arrange
        _sut.AddColumns("Col1", "Col2");
        var row = new KeyValuePair<int, List<TableCell>>(0, []);

        // Act & Assert
        var ex = Assert.ThrowsAny<ArgumentException>(() => _sut.AddRow(row, "A"));
        Assert.True(ex.Message.Contains("expected ", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ToTableShouldThrowWhenEmpty()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.ToTable());
    }

    [Fact]
    public void ExpandPropertyShouldPropagateToSpectreTable()
    {
        // Arrange
        var builder = new ConsoleTableBuilder() { Expand = true };

        // Act
        var table = builder.AddColumns("A").AddRow(new KeyValuePair<int, List<TableCell>>(0, []), "1").ToTable();

        // Assert
        Assert.True(table.Expand);
    }

    [Fact]
    public void ShowRowSeparatorsPropertyShouldPropagate()
    {
        // Arrange
        var builder = new ConsoleTableBuilder
        {
            ShowRowSeparators = true
        };

        // Act
        var table = builder.AddColumns("A").AddRow(new KeyValuePair<int, List<TableCell>>(0, []), "1").ToTable();

        // Assert
        Assert.True(table.ShowRowSeparators);
    }
}
