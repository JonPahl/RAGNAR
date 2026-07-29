//namespace RAGNAR.UnitTests;

//public sealed class ConfigQuestionLoaderTests
//{
//    //[Fact]
//    //public void LoadFromConfig_CreatesQuestionsWithCorrectState ()
//    //{
//    //    // Arrange
//    //    static Question _factory (string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);

//    //    var loader = new ConfigToQuestionMapper(_factory);


//    //    var configs = new[]
//    //    {
//    //        new Question(true, "Active?", "a", QuestionCategory.Refactor),
//    //        new Question(false, "Disabled?", "d", QuestionCategory.Logging)
//    //    };

//    //    // Act
//    //    var questions = loader.LoadFromConfig(configs);

//    //    // Assert
//    //    Assert.Equal(2, questions.Count);
//    //    Assert.True(questions[0].IsEnabled);
//    //    Assert.False(questions[1].IsEnabled);
//    //}

//    [Fact]
//    public void LoadFromConfig_Throws_WhenConfigsNull ()
//    {
//        // Arrange
//        static Question _factory (string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);
//        var loader = new ConfigToQuestionMapper(_factory);

//        // Act & Assert
//        Assert.Throws<ArgumentNullException>(() => loader.LoadFromConfig(null!));
//    }

//    [Fact]
//    public void LoadFromConfig_WithEmptyConfigs_ReturnsEmptyList ()
//    {
//        // Arrange
//        static Ragnar.Core.Model.Question _factory (string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);

//        var loader = new ConfigToQuestionMapper(_factory);

//        var configs = Array.Empty<Question>();

//        // Act
//        var questions = loader.LoadFromConfig(configs);

//        // Assert
//        Assert.Empty(questions);
//    }
//}
