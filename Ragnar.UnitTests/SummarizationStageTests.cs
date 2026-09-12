namespace Ragnar.Tests;

public sealed partial class SummarizationStageTests
{
    [Fact]
    public async Task ExecuteAsyncCallsSummarizeAndWritesRule()
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

    [Fact]
    public async Task ExecuteAsyncCallsSummarizeAllResponses()
    {
        var mockSummary = new Mock<ISummaryService>();
        var mockWriter = new Mock<IOutputWriter>();
        var stage = new SummarizationStage(mockSummary.Object, mockWriter.Object);

        await stage.ExecuteAsync(CancellationToken.None);

        mockSummary.Verify(s => s.SummarizeAllResponsesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncWritesRuleAndCompletionText()
    {
        var mockSummary = new Mock<ISummarizeService_Shim>();
        var mockSummaryReal = new Mock<ISummaryService>();
        var mockWriter = new Mock<IOutputWriter>();
        var stage = new SummarizationStage(mockSummaryReal.Object, mockWriter.Object);

        await stage.ExecuteAsync(CancellationToken.None);

        mockWriter.Verify(w => w.WriteRule(), Times.AtLeastOnce);
        mockWriter.Verify(w => w.MarkupLine(It.Is<string>(s => s.Contains("Questions Finished"))), Times.Once);
    }

    //[Fact]
    //public async Task ExecuteAsync_PropagatesCancellation()
    //{
    //    var mockSummary = new Mock<ISummaryService>();
    //    var mockWriter = new Mock<IOutputWriter>();
    //    var stage = new SummarizationStage(mockSummary.Object, mockWriter.Object);

    //    using var cts = new CancellationTokenSource();
    //    cts.Cancel();

    //    await Assert.ThrowsAnyAsync<canceledExceptionWrapper>(
    //        () => stage.ExecuteAsync(cts.Token))
    //        .OrAwait(() => { /* may not throw – just verify no exception */ });
    //}

    [Fact]
    public void SummarizationStageImplementsIPipelineStage()
    {
        var mockSummary = new Mock<ISummaryService>().Object;
        var mockWriter = new Mock<IOutputWriter>().Object;
        var stage = new SummarizationStage(mockSummary, mockWriter);

        Assert.IsType<IPipelineStage>(stage, exactMatch: false);
    }
}
