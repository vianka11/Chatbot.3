using Chatbot3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Chatbot3.Views
{
    public partial class QuizWindow : Window
    {
        // List to store quiz questions
        private List<QuizQuestion> questions;
        // Index of current question
        private int currentIndex = 0;
        // Tracks user's score
        private int score = 0;

        // Constructor: Initializes window, loads questions and displays the first one
        public QuizWindow()
        {
            InitializeComponent();
            LoadQuestions();
            DisplayQuestion();
        }
        // Predefined list of cybersecurity quiz questions
        private void LoadQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion("What should you do if you receive an email asking for your password?",
                    new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    2,
                    "Correct! Reporting phishing emails helps prevent scams."),

                new QuizQuestion("True or False: It's safe to use the same password for all accounts.",
                    new List<string> { "True", "False" },
                    1,
                    "Correct! You should use unique passwords for each account."),

                new QuizQuestion("Which is NOT a strong password?",
                    new List<string> { "P@ssw0rd!", "123456", "G7#dk9!r", "LimeApple!42" },
                    1,
                    "123456 is one of the most common and unsafe passwords."),

                new QuizQuestion("Why is two-factor authentication (2FA) important?",
                    new List<string> { "It makes login slower", "It adds an extra layer of security", "It's optional", "It tracks you online" },
                    1,
                    "2FA protects your account even if your password is compromised."),

                new QuizQuestion("How can you tell if a link might be malicious?",
                    new List<string> { "It starts with https", "It has spelling errors", "It’s sent by a friend", "It ends in .com" },
                    1,
                    "Spelling errors and strange characters are red flags for malicious links."),

                new QuizQuestion("You see a post claiming you won a prize—what do you do?",
                    new List<string> { "Click and enter your details", "Report it", "Forward it to friends", "Ignore warnings" },
                    1,
                    "It's likely a scam—report and do not engage."),

                new QuizQuestion("What does phishing aim to steal?",
                    new List<string> { "Fish", "Cookies", "Sensitive personal data", "Wi-Fi signal" },
                    2,
                    "Phishing attacks try to trick you into giving up sensitive info."),

                new QuizQuestion("True or False: Public Wi-Fi networks are always safe.",
                    new List<string> { "True", "False" },
                    1,
                    "False! Public Wi-Fi can be easily intercepted by attackers."),

                new QuizQuestion("Which of the following is good browser hygiene?",
                    new List<string> { "Saving passwords in plain text", "Using incognito mode always", "Clearing cache regularly", "Ignoring updates" },
                    2,
                    "Clearing cache and keeping your browser updated improve security."),

                new QuizQuestion("Which technique is common in social engineering?",
                    new List<string> { "Phishing emails", "Captcha solving", "Encryption", "Firewall alerts" },
                    0,
                    "Phishing is one of the most common ways to manipulate users into revealing secrets.")
            };

            // Shuffle questions for variety
            questions = questions.OrderBy(q => Guid.NewGuid()).ToList();
        }
        // Displays the current question and its answer options
        private void DisplayQuestion()
        {
            // Show the score
            if (currentIndex >= questions.Count)
            {
                ShowFinalScore();
                return;
            }

            var question = questions[currentIndex];
            QuestionText.Text = $"Q{currentIndex + 1}: {question.Question}";
            OptionsList.ItemsSource = question.Options;
            OptionsList.SelectedIndex = -1;
            FeedbackText.Text = "";
            SubmitButton.IsEnabled = true;
        }

        // Handles logic when the user submits an answer
        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            // Check if user selected an option
            if (OptionsList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an option.");
                return;
            }

            var question = questions[currentIndex];
            bool isCorrect = OptionsList.SelectedIndex == question.CorrectIndex;

            // Increment score if correct
            if (isCorrect)
                score++;

            // Feedback based on correctness
            FeedbackText.Text = isCorrect
                ? $"✅ {question.Explanation}"
                : $"❌ Incorrect. {question.Explanation}";

            // Update score 
            ScoreText.Text = $"Score: {score} / {questions.Count}";

            // Move to next question
            currentIndex++;
            SubmitButton.IsEnabled = false;

            // Wait and then show next question
            Dispatcher.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(1500);
                DisplayQuestion();
            });
        }

        // Displays final result 
        private void ShowFinalScore()
        {
            QuestionText.Text = "🎉 Quiz complete!";
            OptionsList.Visibility = Visibility.Collapsed;
            SubmitButton.Visibility = Visibility.Collapsed;

            // Feedback based on user's score
            FeedbackText.Text = score >= 8
                ? "Fantastic job! You're a cybersecurity pro! 🛡️"
                : score >= 5
                    ? "Great effort! Keep brushing up your skills. 💡"
                    : "Don't worry—security starts with learning, and you're on your way! 📘";

            ScoreText.Text = $"Final Score: {score} / {questions.Count}";
        }
    }
}

// Troelson, A. and Japikse P., 2022. Pro C# 10 with .NET 6. 11th ed. California: Apress.
