//using Google.Protobuf.WellKnownTypes;
//using Qdrant.Client.Grpc;
//using Ragnar.Questions.Questions.Filters;

//namespace RAGNAR.UnitTests;

//public class QuestionFilterFactoryTests
//{
//    [Theory]
//    [InlineData(QuestionCategory.XML, 0, typeof(XmlCommentFilterStrategy))]
//    [InlineData(QuestionCategory.XML, 10, typeof(XmlCommentLengthFilterStrategy))]
//    [InlineData(QuestionCategory.Other, 0, typeof(Filter))]
//    public void FindFilter_ReturnsCorrectFilterType (QuestionCategory category, int size, Google.Protobuf.Reflection.FieldDescriptorProto.Types.Type expectedType)
//    {
//        // Act
//        var filter = QuestionFilterFactory.FindFilter(category, size);

//        // Assert
//        Assert.IsType(expectedType, filter);
//    }
//}
