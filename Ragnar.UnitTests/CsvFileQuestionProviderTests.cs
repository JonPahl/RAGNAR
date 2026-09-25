// ═══════════════════════════════════════════════════════════
// VectorStoreRepositoryTests.cs
// ═══════════════════════════════════════════════════════════
namespace Ragnar.Tests;

public class CsvFileQuestionProviderTests
{
    private readonly Mock<IRecordParser<QuestionRecord>> _parserMock;
    private readonly CsvQuestionSource _sut;

    public CsvFileQuestionProviderTests()
    {
        _parserMock = new Mock<IRecordParser<QuestionRecord>>();
        _sut = new CsvQuestionSource(_parserMock.Object);
    }

    [Fact]
    public void ProviderNameShouldReturnCsvFile()
    {
        Assert.Equal("CSV File", _sut.ProviderName);
    }

    [Fact]
    public async Task LoadQuestionsAsyncValidRecordsShouldMapToQuestions()
    {
        // Arrange
        var records = new List<QuestionRecord>
        {
            new() { Text = "What is C#?", IsEnabled = true, FileName = "q.csv", Category = QuestionCategory.Other },
            new() { Text = "LINQ?", IsEnabled = false, FileName = "q.csv", Category = QuestionCategory.Other }
        };
        _parserMock
            .Setup(p => p.ParseAsync("data.csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = (await _sut.LoadQuestionsAsync("data.csv", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("What is C#?", result[0].Text);
        Assert.True(result[0].IsActive);
        Assert.Equal("q.csv", result[0].FileName);
        Assert.Equal(QuestionCategory.Other, result[0].Category);
        Assert.False(result[1].IsActive);
    }

    [Fact]
    public async Task LoadQuestionsAsyncEmptyRecordsShouldReturnEmptyCollection()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = (await _sut.LoadQuestionsAsync("empty.csv", CancellationToken.None)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadQuestionsAsyncParserThrowsShouldPropagateException()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("File not found"));

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _sut.LoadQuestionsAsync("missing.csv", CancellationToken.None));
    }

    [Fact]
    public async Task LoadQuestionsAsyncCancellationRequestedShouldThrow()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.LoadQuestionsAsync("data.csv", CancellationToken.None));
    }
}
