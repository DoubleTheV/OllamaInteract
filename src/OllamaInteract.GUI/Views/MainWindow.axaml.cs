using System;
using Avalonia.Controls;
using Avalonia.Input;
using OllamaInteract.GUI.ViewModels;

namespace OllamaInteract.GUI.Views;

public partial class MainWindow : Window
{
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
}