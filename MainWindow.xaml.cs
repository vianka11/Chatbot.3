// Add the event handler for DeleteTask_Click
private void DeleteTask_Click(object sender, RoutedEventArgs e)
{
    if (TaskListBox.SelectedItem != null)
    {
        TaskListBox.Items.Remove(TaskListBox.SelectedItem);
    }
    else
    {
        MessageBox.Show("Please select a task to delete.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}