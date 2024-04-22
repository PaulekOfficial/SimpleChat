using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Core;
using SimleChatClient.ViewModel;
using SimpleChatClient;
using SimpleChatProtocol;

namespace ChatClientGUI.Models.ViewModel;

public class LoginViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();
    
    private string _errorMessage;
    private string _username;
    private string _password;
    
    public ICommand LoginCommand { get; private set; }

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(Login, CanLogin);
        
        _client.ChannelHandler.GetPacketManager()
            .RegisterHandler<ServerSidePackets.LoginFailedPacket>(0x1C, HandleLoginFailedPacket);
    }

    public void HandleLoginFailedPacket(ServerSidePackets.LoginFailedPacket packet, EventArgs eventArgs)
    {
        ErrorMessage = packet.Reason;
        
        string messageBoxText2 = "Wystąpił błąd podczas logowania. Spróbuj ponownie.";
        string caption2 = "System Logowania";
        MessageBox.Show(messageBoxText2, caption2, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
    }

    private void Login(object parameter)
    {
        var passwordBox = parameter as PasswordBox;
        if (passwordBox == null)
        {
            ErrorMessage = "You must provide a password.";
            return;
        }
        
        string messageBoxText2 = "Logowanie... Proszę czekać.";
        string caption2 = "System Logowania";
        MessageBox.Show(messageBoxText2, caption2, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
        
        var packet = new LoginRequestPacket
        {
            Login = Username,
            Password = Password
        };
        
        _client.SendPacket(packet);
    }

    public void PasswordChanged(PasswordBox passwordBox, RoutedEventArgs e)
    {
        Password = passwordBox.Password;
    }

    public void UsernameChanged(TextBox textBox, TextChangedEventArgs e)
    {
        Username = textBox.Text;
    }
    
    private bool CanLogin(object parameter)
    {
        return true;
    }
    
    public string ErrorMessage
    {
        get { return _errorMessage; }
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }
    
    public string Username
    {
        get { return _username; }
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }
    
    public string Password
    {
        get { return _password; }
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }
}