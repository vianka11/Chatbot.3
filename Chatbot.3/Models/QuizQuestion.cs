using System.Collections.Generic;

namespace Chatbot3.Models
{
    // Represents a quiz question,
    // possible options, correct answer index and explanation.
    public class QuizQuestion
    {
        // Question.
        public string Question { get; set; }
        // List of possible answer choices.
        public List<string> Options { get; set; }
        // Index of the correct option 
        public int CorrectIndex { get; set; }
        // Explanation shown after answering the question.
        public string Explanation { get; set; }
        // Constructor to initialize a new quiz question with options and an explanation.
        public QuizQuestion(string question, List<string> options, int correctIndex, string explanation)
        {
            Question = question;
            Options = options;
            CorrectIndex = correctIndex;
            Explanation = explanation;
        }
    }
}

// Troelson, A. and Japikse P., 2022. Pro C# 10 with .NET 6. 11th ed. California: Apress.