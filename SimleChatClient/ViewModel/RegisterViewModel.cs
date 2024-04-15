using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Core;
using SimpleChatProtocol;

namespace SimleChatClient.ViewModel;

public class RegisterViewModel : ObservableObject
{
    private string _errorMessage;
    private string _username;
    private string _password;
    private string _confirmPassword;
    
    public ICommand RegisterCommand { get; private set; }

    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(Register, CanRegister);
    }

    private bool CanRegister(object parameter)
    {
        return true;
    }

    private void Register(object parameter)
    {
        var passwordBox = parameter as PasswordBox;
        if (passwordBox == null)
            return;

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        //TODO check if username is valid and not taken lowercase also
        
        var packet = new RegisterRequestPacket
        {
            Login = Username,
            Password = Password
        };
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
}