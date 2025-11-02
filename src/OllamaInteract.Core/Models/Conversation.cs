using System.ComponentModel;

namespace OllamaInteract.Core.Models;

public class Conversation : INotifyPropertyChanged
{
    public uint ID { get; private set; }

    private string _name { get; set; } = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if(_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public Conversation(uint id)
    {
        ID = id;
        Name = "New conversation";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}