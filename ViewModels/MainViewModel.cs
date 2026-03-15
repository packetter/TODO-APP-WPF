using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using TODO_APP_WPF.Models;

namespace TODO_APP_WPF.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private static readonly string SaveFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TODO-APP-WPF", "todos.json");

    private string _inputText = string.Empty;

    public ObservableCollection<Todo> Todos { get; } = [];

    public string InputText
    {
        get => _inputText;
        set
        {
            if (_inputText != value)
            {
                _inputText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputText)));
            }
        }
    }

    public int RemainingCount => Todos.Count(t => !t.Completed);

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        AddCommand = new RelayCommand(_ => AddTodo(), _ => !string.IsNullOrWhiteSpace(InputText));
        DeleteCommand = new RelayCommand(id => DeleteTodo((int)id!));

        LoadTodos();
        Todos.CollectionChanged += (_, _) => OnTodosChanged();
    }

    private void AddTodo()
    {
        var trimmed = InputText.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        var todo = new Todo
        {
            Id = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Text = trimmed,
            Completed = false
        };
        todo.PropertyChanged += (_, _) => OnTodosChanged();
        Todos.Add(todo);
        InputText = string.Empty;
    }

    private void DeleteTodo(int id)
    {
        var todo = Todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) Todos.Remove(todo);
    }

    private void OnTodosChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingCount)));
        SaveTodos();
    }

    private void SaveTodos()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
        var json = JsonSerializer.Serialize(Todos.ToList());
        File.WriteAllText(SaveFilePath, json);
    }

    private void LoadTodos()
    {
        if (!File.Exists(SaveFilePath)) return;
        try
        {
            var json = File.ReadAllText(SaveFilePath);
            var todos = JsonSerializer.Deserialize<List<Todo>>(json);
            if (todos == null) return;
            foreach (var todo in todos)
            {
                todo.PropertyChanged += (_, _) => OnTodosChanged();
                Todos.Add(todo);
            }
        }
        catch (Exception)
        {
            // 読み込み失敗時は空の状態で開始
        }
    }
}
