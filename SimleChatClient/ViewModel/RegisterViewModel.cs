using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Core;
using ChatClientGUI.Models.ViewModel;
using ChatClientGUI.View;
using SimpleChatClient;
using SimpleChatProtocol;

namespace SimleChatClient.ViewModel;

public class RegisterViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();
    
    private string _errorMessage;
    private string _username;
    private string _password;
    private string _confirmPassword;
    private bool _isButtonEnabled;
    
    public ICommand RegisterCommand { get; private set; }

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(Register, CanRegister);
        _isButtonEnabled = true;
        
        _client.ChannelHandler.GetPacketManager()
            .RegisterHandler<ServerSidePackets.UsernameCheckResponsePacket>(0x1B, HandleUsernameCheckResponsePacket);
    }

    private bool CanRegister(object parameter)
    {
        return true;
    }

    private void Register(object parameter)
    {
        var passwordBox = parameter as PasswordBox;
        if (passwordBox == null)
        {
            ErrorMessage = "You must provide a password.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }
        
        if (Password.Length < 8) 
        {
            ErrorMessage = "Password is too short. Min 8 chars";
            return;
        }
        
        if (Password.Length > 40) 
        {
            ErrorMessage = "Password is too long. Max 40 chars";
            return;
        }
        
        if (!Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$"))
        {
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character.";
            return;
        }
        
        IsButtonEnabled = false;

        string messageBoxText = "Wysłano proźbę o weryfikację pseudonimu. Proszę czekać na weryfikację.";
        string caption = "Weryfikacja";
        MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
        
        var packet = new UsernameCheckRequestPacket
        {
            Username = Username
        };
        
        _client.SendPacket(packet);
    }
    
    public void HandleUsernameCheckResponsePacket(ServerSidePackets.UsernameCheckResponsePacket packet, EventArgs eventArgs)
    {
        if (!packet.Exists)
        {
            ErrorMessage = "Username already exists.";
            IsButtonEnabled = true;
            
            string messageBoxText = "Istnieje użytkownik o takiej nazwie. Proszę podać inną nazwę.";
            string caption = "Weryfikacja";
            MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
            return;
        }
       
        var registerRequest = new RegisterRequestPacket
        {
            Login = Username,
            Password = Password
        };
        _client.SendPacket(registerRequest);
        
        string messageBoxText2 = "Rejestracja przebiegła pomyślnie. Możesz się teraz zalogować.";
        string caption2 = "Rejestracja";
        MessageBox.Show(messageBoxText2, caption2, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);

        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainDataContext = Application.Current.MainWindow.DataContext as MainViewModel;
            if (mainDataContext == null) return;

            mainDataContext.SelectedViewModel = new LoginView();
        });
    }
    
    public void PasswordChanged(PasswordBox passwordBox, RoutedEventArgs e)
    {
        Password = passwordBox.Password;
    }

    public void ConfirmPasswordChanged(PasswordBox passwordBox, RoutedEventArgs e)
    {
        ConfirmPassword = passwordBox.Password;
    }

    public void UsernameChanged(TextBox textBox, TextChangedEventArgs e)
    {
        Username = textBox.Text;
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

    public string ConfirmPassword
    {
        get { return _confirmPassword; }
        set
        {
            _confirmPassword = value;
            OnPropertyChanged(nameof(ConfirmPassword));
        }
    }
    
    public bool IsButtonEnabled
    {
        get { return _isButtonEnabled; }
        set
        {
            _isButtonEnabled = value;
            OnPropertyChanged(nameof(IsButtonEnabled));
        }
    }
}