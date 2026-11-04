namespace Ragnar.Tests;

public class NumberedRowDecoratorTests
{
    [Fact]
    public void TableShouldDelegateToInnerRenderer()
    {
        var inner = new Mock<ITableRenderer>();
        var builder = new ConsoleTableBuilder();
        inner.SetupGet(r => r.Table).Returns(builder);

        var decorator = new NumberedRowDecorator(inner.Object);

        Assert.Same(builder, decorator.Table);
    }

    [Fact]
    public void RenderShouldCallInnerRenderExactlyOnce()
    {
        var inner = new Mock<ITableRenderer>();
        inner.SetupGet(r => r.Table).Returns(new ConsoleTableBuilder());

        var decorator = new NumberedRowDecorator(inner.Object);

        decorator.Render();
        decorator.Render(); // second call should be a no-op

        inner.Verify(r => r.Render(), Times.Once);
    }
}
