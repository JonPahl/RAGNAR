namespace Ragnar.Tests.Providers;

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
            new() { IsEnabled = true, Text = "What is C#?", FileName = "q.csv", Category = QuestionCategory.Other },
            new() { IsEnabled = false, Text = "Disabled Q", FileName = "q.csv", Category = QuestionCategory.XML }
        };
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var questions = (await _sut.LoadQuestionsAsync("test.csv", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, questions.Count);
        Assert.True(questions[0].IsActive);
        Assert.Equal("What is C#?", questions[0].Text);
        Assert.False(questions[1].IsActive);
        _parserMock.Verify(p => p.ParseAsync("test.csv", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadQuestionsAsyncEmptyRecordsShouldReturnEmptyList()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var questions = await _sut.LoadQuestionsAsync("empty.csv", CancellationToken.None);

        // Assert
        Assert.Empty(questions);
    }

    [Fact]
    public async Task LoadQuestionsAsyncParserThrowsShouldPropagate()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("File not found"));

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _sut.LoadQuestionsAsync("missing.csv", CancellationToken.None));
    }
}
