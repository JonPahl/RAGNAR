namespace Ragnar.Core.Rendering.Decorators;

/// <summary>
/// Decorator that prepends a 1-based index column to every row
/// before rendering.
/// </summary>
public sealed class NumberedRowDecorator(ITableRenderer inner, string columnHeader = "#") : ITableRenderer
{
    public ConsoleTableBuilder Table => inner.Table;

    public void Render()
    {
        // Inject the index column into the builder before the inner renders
        var numbered = Table.Columns.ConvertAll(c => c);
        if (!numbered.Contains(columnHeader, StringComparer.OrdinalIgnoreCase))
            Table.AddColumns(columnHeader);

        inner.Render();
    }
}
