using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OllamaInteract.GUI.ViewModels;
using Tmds.DBus.Protocol;

namespace OllamaInteract.GUI.Views;

public partial class MainWindow : Window
{
    private string _previousConversationName = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void HandleInputFunctions(object sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.Enter) // add newline
        {
            var textBox = (TextBox)sender;
            int caretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text?.Insert(caretIndex, "\n");
            textBox.CaretIndex = caretIndex + 1;
            e.Handled = true;
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V) // paste
        {
            var clipboardText = Clipboard?.GetTextAsync().Result;
            if (!string.IsNullOrEmpty(clipboardText))
            {
                var textBox = (TextBox)sender;
                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text?.Insert(caretIndex, clipboardText);
                textBox.CaretIndex = caretIndex + clipboardText.Length;
            }
            e.Handled = true;
        }
        else if (DataContext != null)
        {
            e.Handled = ((MainWindowViewModel)DataContext).InputHandler(e);
            return;
        }
        e.Handled = false;
    }

    private void StartedNameChange(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        if (textBox != null && textBox.Text != null)
        {
            _previousConversationName = textBox.Text;
            e.Handled = true;
            return;
        }
        e.Handled = false;
    }

    private void ConversationNameChanged(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        if (textBox != null && textBox.Text != null)
        {
            if (_previousConversationName != textBox.Text && DataContext != null)
            {
                e.Handled = ((MainWindowViewModel)DataContext).UpdateConversationName();
                return;
            }
        }
        e.Handled = false;
    }

    private void BackgroundClicked(object sender, PointerPressedEventArgs e)
    {
        if (ConversationNameField.IsKeyboardFocusWithin)
        {
            UserPromptInputElement.Focus();
            e.Handled = true;
            return;
        }
        e.Handled = false;
    }
}