using Chatbot3.Models;
using Chatbot3.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Chatbot3
{
    public partial class MainWindow : Window
    {
        // User-specific state
        private string userName = "";
        private string userInterest = "";
        private string currentTopic = "";

        // Task list and activity log
        private readonly List<TaskItem> tasks = new List<TaskItem>();
        private readonly List<string> activityLog = new List<string>();

        // Last task added 
        private TaskItem lastAddedTask = null;
        // Flag indicating if chatbot is waiting for reminder confirmation response
        private bool waitingForReminderConfirmation = false;

        // Sentiment mapping keywords to empathetic responses
        private readonly Dictionary<string, string> sentimentMap = new Dictionary<string, string>
        {
            { "worried", "It’s okay to feel that way. Let’s tackle it together." },
            { "frustrated", "Cybersecurity can be confusing—I'm here to make it easier." },
            { "curious", "Curiosity is a superpower when learning about digital safety!" }
        };
        // Keywords mapped to lists of cybersecurity tips
        private readonly Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>
        {
            { "password", new List<string> {
                "Use strong, unique passwords for every account.",
                "Avoid using personal info like birthdays in your passwords.",
                "Consider using a password manager to store credentials securely." }},
            { "privacy", new List<string> {
                "Check your social media privacy settings regularly.",
                "Think twice before posting sensitive personal information online.",
                "Limit app permissions to only what's necessary." }},
            { "scam", new List<string> {
                "Don’t click suspicious links or download unknown files.",
                "Scammers may impersonate people you trust—verify before responding.",
                "Avoid sharing passwords over email or chat." }},
            { "phishing", new List<string> {
                "Watch for fake login pages that mimic real websites.",
                "If in doubt, navigate directly to the website instead of clicking a link.",
                "Phishing emails often pressure you to act immediately—slow down and think." }}
        };
        // Question keywords mapped to canned answers
        private readonly Dictionary<string, string> simpleQuestions = new Dictionary<string, string>
        {
            { "how are you", "I'm fully operational and ready to help you stay cyber safe!" },
            { "what's your purpose", "I'm here to educate and assist you with cybersecurity awareness." },
            { "what can i ask you about", "You can ask me about password safety, phishing, privacy, and general safe browsing practices." },
            { "safe browsing", "Avoid suspicious websites, keep your browser updated, and don't save passwords on public computers." }
        };
        // Constructor initializes components
        public MainWindow()
        {
            InitializeComponent();
        }
        // When window loads, play audio greeting, display ASCII art, and prompt for name
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayWelcomeAudio();
            DisplayAsciiArt();
            AppendBotMessage("👋 Hello there! Please enter your name to begin:");
        }
        // Play welcome audio; catch failures gracefully
        private void PlayWelcomeAudio()
        {
            try
            {
                new SoundPlayer("C:\\Users\\viank\\source\\repos\\Chatbot.3\\Chatbot.3\\welcome_message.wav").Play();
            }
            catch
            {
                AppendBotMessage("⚠ Unable to play welcome audio.");
            }
        }
        // Show ASCII art 
        private void DisplayAsciiArt()
        {
            AsciiArtBlock.Text = @"
  ____      _               ____            _     _       
 / ___|   _| |__   ___ _ __| __ ) _   _  __| | __| |_   _ 
| |  | | | | '_ \ / _ \ '__|  _ \| | | |/ _` |/ _` | | | |
| |__| |_| | |_) |  __/ |  | |_) | |_| | (_| | (_| | |_| |
 \____\__, |_.__/ \___|_|  |____/ \__,_|\__,_|\__,_|\__, |
      |___/                                         |___/ 
     Cybersecurity Awareness Bot
====================================================";
        }
        // Handle Start Chat button click: set user name and greet
        private void StartChat_Click(object sender, RoutedEventArgs e)
        {
            userName = UserNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                GreetingBlock.Text = "👋 Please enter your name to begin.";
                return;
            }

            GreetingBlock.Text = $"Welcome, {userName}! I'm here to help you stay safe online.";
            activityLog.Add($"{DateTime.Now:HH:mm} - Name entered: {userName}");
            AppendBotMessage($"Welcome, {userName}! 😊 Let's start your cybersecurity journey.");
        }
        // Handle Send button click or enter key pressed in input box
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendBotMessage("I'm here when you're ready!");
                return;
            }

            AppendUserMessage($"{userName}: {input}");
            UserInputBox.Text = "";
            // Delay for natural typing effect
            await Task.Delay(300);

            string lowerInput = input.ToLower();
            // Handle reminder confirmation flow for last added task
            if (waitingForReminderConfirmation && lastAddedTask != null)
            {
                if (lowerInput.Contains("yes"))
                {
                    AppendBotMessage("When should I remind you? (e.g., 'tomorrow', 'in 2 days')");
                    waitingForReminderConfirmation = false;
                    return;
                }
                else if (lowerInput.Contains("no"))
                {
                    AppendBotMessage("Okay, no reminder set.");
                    waitingForReminderConfirmation = false;
                    lastAddedTask = null;
                    return;
                }
                else
                {
                    AppendBotMessage("Please answer 'yes' or 'no'. Would you like to set a reminder?");
                    return;
                }
            }

            // Parse reminder date for last added task if applicable
            if (lastAddedTask != null && !lastAddedTask.ReminderDate.HasValue)
            {
                DateTime? reminderDate = ParseReminderDateFromInput(lowerInput);
                if (reminderDate.HasValue)
                {
                    lastAddedTask.ReminderDate = reminderDate;
                    RefreshTaskList();
                    AppendBotMessage($"🔔 Reminder set for '{lastAddedTask.Description}' on {reminderDate:dd MMM yyyy}.");
                    activityLog.Add($"{DateTime.Now:HH:mm} - Reminder set: {lastAddedTask.Description}");
                    lastAddedTask = null;
                    return;
                }
                else
                {
                    AppendBotMessage("Sorry, I didn't understand the date. Try 'tomorrow' or 'in 2 days'.");
                    return;
                }
            }

            // If userName is still empty, treat input as name
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = input;
                GreetingBlock.Text = $"Welcome, {userName}!";
                AppendBotMessage($"Nice to meet you, {userName}! Ask me anything about cybersecurity.");
                activityLog.Add($"{DateTime.Now:HH:mm} - Name entered via chat: {userName}");
                return;
            }

            // Handle input with various handlers; if none match, reply unclear
            if (HandleInterestStatement(lowerInput)) return;
            if (HandleSentiment(lowerInput)) return;
            if (HandleActivityLogRequest(lowerInput)) return;
            if (HandleQuizCommand(lowerInput)) return;
            if (HandleTaskInput(lowerInput)) return;
            if (HandleMemoryCommands(lowerInput)) return;
            if (HandleKeywordResponse(lowerInput)) return;
            if (HandleSimpleQuestions(lowerInput)) return;

            AppendBotMessage("🤔 Hmm, I didn't catch that. Could you rephrase?");
        }

        // Parse reminder date strings 
        private DateTime? ParseReminderDateFromInput(string input)
        {
            if (input.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            var match = Regex.Match(input, @"in (\d+) days?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            return null;
        }

        // Handle adding a new task via input
        private bool HandleTaskInput(string input)
        {
            string[] keywords = { "add task", "create task", "task", "remind me" };
            if (!keywords.Any(k => input.Contains(k))) return false;
            // Remove keywords to isolate task title
            string title = Regex.Replace(input, string.Join("|", keywords), "", RegexOptions.IgnoreCase).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                AppendBotMessage("📝 Please provide a task title.");
                return true;
            }

            // Create task and optionally parse reminder date
            var task = new TaskItem
            {
                Title = title.Length > 40 ? title.Substring(0, 40) + "..." : title,
                Description = title,
                ReminderDate = ParseReminderDateFromInput(input)
            };

            tasks.Add(task);
            RefreshTaskList();
            lastAddedTask = task;

            if (task.ReminderDate.HasValue)
            {
                AppendBotMessage($"✅ Task '{task.Title}' added with reminder on {task.ReminderDate:dd MMM yyyy}.");
                lastAddedTask = null;
            }
            else
            {
                AppendBotMessage($"📝 Task '{task.Title}' added. Would you like a reminder? (yes/no)");
                waitingForReminderConfirmation = true;
            }

            activityLog.Add($"{DateTime.Now:HH:mm} - Task added: {task.Title}");
            return true;
        }

        // Handle user declaring an interest
        private bool HandleInterestStatement(string input)
        {
            if (input.StartsWith("i'm interested in ") || input.StartsWith("i am interested in "))
            {
                userInterest = input.Split(new[] { "in " }, StringSplitOptions.None).Last().Trim();
                AppendBotMessage($"Noted! You're interested in {userInterest}. 👍");
                activityLog.Add($"{DateTime.Now:HH:mm} - User interest set: {userInterest}");
                return true;
            }
            return false;
        }

        // Detect sentiment keywords and reply empathetically
        private bool HandleSentiment(string input)
        {
            foreach (var mood in sentimentMap)
            {
                if (input.Contains(mood.Key))
                {
                    AppendBotMessage($"🧠 {mood.Value}");
                    return true;
                }
            }
            return false;
        }
        // Handle keyword-based cybersecurity tips and more requests
        private bool HandleKeywordResponse(string input)
        {
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    currentTopic = keyword;
                    var tip = keywordResponses[keyword].OrderBy(_ => Guid.NewGuid()).First();
                    AppendBotMessage($"🔐 {tip}");
                    return true;
                }
            }

            // Provide more tips on current topic 
            if (input.Contains("more") && !string.IsNullOrEmpty(currentTopic))
            {
                var tip = keywordResponses[currentTopic].OrderBy(_ => Guid.NewGuid()).First();
                AppendBotMessage($"💡 More on {currentTopic}: {tip}");
                return true;
            }

            return false;
        }

        // Answer simple questions
        private bool HandleSimpleQuestions(string input)
        {
            foreach (var q in simpleQuestions)
            {
                if (input.Contains(q.Key))
                {
                    AppendBotMessage(q.Value);
                    return true;
                }
            }
            return false;
        }

        // Remember user interest 
        private bool HandleMemoryCommands(string input)
        {
            if (input.Contains("remember") && !string.IsNullOrEmpty(currentTopic))
            {
                userInterest = currentTopic;
                AppendBotMessage($"Got it! I’ll remember your interest in {userInterest}.");
                return true;
            }

            if (input.Contains("what do you remember") && !string.IsNullOrEmpty(userInterest))
            {
                AppendBotMessage($"You're interested in {userInterest}.");
                return true;
            }

            return false;
        }

        // Activity log
        private bool HandleActivityLogRequest(string input)
        {
            if (input.Contains("activity log") || input.Contains("what have you done"))
            {
                if (activityLog.Count == 0)
                {
                    AppendBotMessage("I have no logged actions yet.");
                    return true;
                }

                AppendBotMessage("📋 Here's your recent activity:");
                // Show up to last 5 entries in grey
                foreach (var entry in activityLog.Skip(Math.Max(0, activityLog.Count - 5)))
                {
                    AppendBotMessage($"- {entry}", Colors.Gray);
                }

                return true;
            }

            return false;
        }

        // Launch quiz window 
        private bool HandleQuizCommand(string input)
        {
            if (input.Contains("quiz"))
            {
                new QuizWindow { Owner = this }.ShowDialog();
                activityLog.Add($"{DateTime.Now:HH:mm} - Quiz launched.");
                return true;
            }
            return false;
        }

        // Refresh task list, sorting incomplete tasks first
        private void RefreshTaskList()
        {
            TaskListBox.ItemsSource = null;
            TaskListBox.ItemsSource = tasks.OrderBy(t => t.IsCompleted).ToList();
        }

        // Mark task as complete
        private void MarkTaskComplete_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selected)
            {
                selected.IsCompleted = true;
                RefreshTaskList();
                MessageBox.Show($"✅ Task marked complete: {selected.Title}");
                activityLog.Add($"{DateTime.Now:HH:mm} - Completed: {selected.Title}");
            }
        }

        // Delete task 
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selected)
            {
                tasks.Remove(selected);
                RefreshTaskList();
                AppendBotMessage($"🗑️ Task deleted: {selected.Title}");
                activityLog.Add($"{DateTime.Now:HH:mm} - Deleted: {selected.Title}");
            }
        }

        // Launch quiz 
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            new QuizWindow { Owner = this }.ShowDialog();
            activityLog.Add($"{DateTime.Now:HH:mm} - Quiz launched via button.");
        }

        // Message Helpers
        private void AppendBotMessage(string text, Color? color = null)
        {
            AppendMessage($"Bot: {text}", color ?? Colors.DarkBlue);
        }

        // Helper to append user messages 
        private void AppendUserMessage(string text)
        {
            AppendMessage(text, Colors.Black);
        }

        // Core helper to append text to the chat output RichTextBox with color
        private void AppendMessage(string text, Color color)
        {
            var paragraph = new Paragraph(new Run(text + "\n"))
            {
                Foreground = new SolidColorBrush(color)
            };
            ChatOutputBlock.Document.Blocks.Add(paragraph);
        }
    }
}

// Troelson, A. and Japikse P., 2022. Pro C# 10 with .NET 6. 11th ed. California: Apress.