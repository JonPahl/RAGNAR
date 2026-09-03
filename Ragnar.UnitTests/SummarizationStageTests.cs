namespace Ragnar.Tests;

public sealed class SummarizationStageTests
{
    [Fact]
    public async Task ExecuteAsync_CallsSummarizeAndWritesRule()
    {
        // Arrange
        var summaryMock = new Mock<ISummaryService>();
        var writerMock = new Mock<IOutputWriter>();
        var stage = new SummarizationStage(summaryMock.Object, writerMock.Object);

        // Act
        await stage.ExecuteAsync(CancellationToken.None);

        // Assert
        summaryMock.Verify(s => s.SummarizeAllResponsesAsync(It.IsAny<CancellationToken>()), Times.Once);
        writerMock.Verify(w => w.WriteRule(), Times.AtLeastOnce);
    }
}
