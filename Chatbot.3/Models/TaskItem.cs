using System;

namespace Chatbot3.Models
{
    // Represents a task item with a title, description, optional reminder date, and completion status.
    public class TaskItem
    {
        // Title of task.
        public string Title { get; set; }

        // Description task.
        public string Description { get; set; }
        // Optional date to remind the user about this task.
        // Nullable to indicate no reminder is set.
        public DateTime? ReminderDate { get; set; }
        // Indicates whether the task has been completed.
        public bool IsCompleted { get; set; }
        // Returns a string representation of the task, showing completion status,
        // title and reminder date 
        public override string ToString()
        {
            // Show checkmark if completed, empty box otherwise
            string status = IsCompleted ? "[✔]" : "[ ]";
            // Append reminder date 
            string reminder = ReminderDate.HasValue ? $" (Remind: {ReminderDate:dd MMM yyyy})" : "";
            return $"{status} {Title}{reminder}";
        }
    }
}

// Troelson, A. and Japikse P., 2022. Pro C# 10 with .NET 6. 11th ed. California: Apress.