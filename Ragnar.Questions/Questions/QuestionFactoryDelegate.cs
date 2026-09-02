namespace Ragnar.Questions.Questions;
/// <summary>
/// Delegate for creating a Question from parts.
/// </summary>
/// <param name="text">The question Text.</param>
/// <param name="key">The unique filename/Key.</param>
/// <param name="category">The Category.</param>
/// <param name="isActive">Whether the question is active.</param>
/// <returns>A Question instance.</returns>
public delegate Core.Model.Question QuestionFactoryDelegate(string text, string key, QuestionCategory category, bool isActive);
