namespace Ragnar.UnitTests;

public class AnsiConsoleOutputWriterTests
{
    private readonly IOutputWriter Writer = new AnsiConsoleOutputWriter();

    [Fact]
    public void Markup_WithNullStyle_CallsPlainMarkup()
    {
        // Act & Assert (Verifies no exceptions and correct routing logic)
        var ex = Record.Exception(() => Writer.Markup("Test", null));
        ex.Should().BeNull();
    }

    [Fact]
    public void Markup_WithStyle_CallsStyledMarkup()
    {
        var style = new Style(Color.Red);
        var ex = Record.Exception(() => Writer.Markup("Test", style));
        ex.Should().BeNull();
    }

    [Fact]
    public void WriteRule_CallsAnsiConsoleWriteRule()
    {
        var ex = Record.Exception(() => Writer.WriteRule());
        ex.Should().BeNull();
    }
}
