using System.ComponentModel;

namespace TODO_APP_WPF.Models;

public class Todo : INotifyPropertyChanged
{
    private bool _completed;

    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public bool Completed
    {
        get => _completed;
        set
        {
            if (_completed != value)
            {
                _completed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Completed)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
