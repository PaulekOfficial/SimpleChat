using System.Windows.Controls;
using ChatClientGUI.Core;
using ChatClientGUI.View;
using SimpleChatClient;

namespace ChatClientGUI.Models.ViewModel;

public class MainViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();

    public MainViewModel()
    {
        SelectedViewModel = new LoginView();
        _windowHeight = 650; // Set initial height
        _windowWidth = 1200; // Set initial width
    }

    private double _windowHeight { get; set; }
    private double _windowWidth { get; set; }
    public UserControl _selectedViewModel { get; set; }

    public UserControl SelectedViewModel
    {
        get => _selectedViewModel;
        set
        {
            _selectedViewModel = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get => _windowHeight;
        set
        {
            _windowHeight = value;
            OnPropertyChanged();
        }
    }

    public double WindowWidth
    {
        get => _windowWidth;
        set
        {
            _windowWidth = value;
            OnPropertyChanged();
        }
    }
}