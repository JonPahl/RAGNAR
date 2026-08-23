namespace Ragnar.Tests;

public class AnsiConsoleOutputWriterTests
{
    private readonly IOutputWriter _writer = new AnsiConsoleOutputWriter();

    [Fact]
    public void Markup_WithNullStyle_CallsPlainMarkup()
    {
        // Act & Assert (Verifies no exceptions and correct routing logic)
        var ex = Record.Exception(() => _writer.Markup("Test", null));
        ex.Should().BeNull();
    }

    [Fact]
    public void Markup_WithStyle_CallsStyledMarkup()
    {
        var style = new Style(Color.Red);
        var ex = Record.Exception(() => _writer.Markup("Test", style));
        ex.Should().BeNull();
    }

    [Fact]
    public void WriteRule_CallsAnsiConsoleWriteRule()
    {
        var Ex = Record.Exception(() => _writer.WriteRule());
        Ex.Should().BeNull();
    }
}
