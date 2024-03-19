using ChatClientGUI.Core;
using SimpleChatClient;

namespace ChatClientGUI.Models.ViewModel;

public class LoginViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();
}