using System.ComponentModel;

public class Folder : INotifyPropertyChanged
{
    private string Name;
    private string Path;

    public string Name
    {
        get => Name;
        set
        {
            Name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Path
    {
        get => Path;
        set
        {
            Path = value;
            OnPropertyChanged(nameof(Path));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
