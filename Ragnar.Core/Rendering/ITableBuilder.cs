namespace Ragnar.Core.Rendering;

public interface ITableBuilder
{
    List<string> Columns { get; }
    bool Expand { get; set; }
    List<string[]> Rows { get; }

    bool ShowRowSeparators { get; set; }

    ConsoleTableBuilder AddColumns(params string[] columns);
    ConsoleTableBuilder AddRow(params string[] cells);
    Table ToTable();
}
