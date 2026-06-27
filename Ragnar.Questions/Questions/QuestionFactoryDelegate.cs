namespace Ragnar.Questions.Questions;
/// <summary>
/// Delegate for creating a Question from parts.
/// </summary>
/// <param name="text">The question text.</param>
/// <param name="key">The unique filename/key.</param>
/// <param name="category">The category.</param>
/// <param name="isActive">Whether the question is active.</param>
/// <returns>A Question instance.</returns>
public delegate Question QuestionFactoryDelegate(string text, string key, QuestionCategory category, bool isActive);
