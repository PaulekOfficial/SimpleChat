using System.Collections.ObjectModel;
using System.Windows.Controls;
using ChatClientGUI.Core;
using ChatClientGUI.Models.Model;
using ChatClientGUI.View;
using SimpleChatClient;

namespace ChatClientGUI.Models.ViewModel;

public class MainViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();
    
    public UserControl SelectedViewModel { get; set; }
    
    public MainViewModel()
    {
        SelectedViewModel = new ChatView();
    }
}