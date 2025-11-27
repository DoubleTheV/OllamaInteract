using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OllamaInteract.Core.Models;
using OllamaInteract.GUI.ViewModels;

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
        if (DataContext != null && ((MainWindowViewModel)DataContext).MenuVisible)
        {
            ((MainWindowViewModel)DataContext).MenuButtonPressed();
        }
        e.Handled = false;
    }

    private void PullModel(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        try
        {
            if (button.Parent is Control &&
                button.Parent.Parent is Control &&
                button.Parent.Parent.DataContext is AvailableModel)
            {
                var model = (AvailableModel)button.Parent.Parent.DataContext;
                var parameter = button.Content;

                var modelName = $"{model}:{parameter}";
                if (DataContext != null)
                {
                    _ = ((MainWindowViewModel)DataContext).PullModel(modelName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GUI Error: {ex.Message}");
        }
    }

    private void EditModel(object sender, RoutedEventArgs e)
    {
        if(DataContext != null)
        {
            try
            {
                if(DataContext != null)
                {
                    var model = ((Button)sender).Tag as string;
                    EditModelParentName.Text = model;
                    ((MainWindowViewModel)DataContext).SetModelWindowModeEdit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GUI Error: {ex.Message}");
            }
        }
    }
}