using System.Windows;
using System.Windows.Controls;
using ChatClientGUI.Models.ViewModel;
using ChatClientGUI.View;
using SimleChatClient.ViewModel;

namespace SimleChatClient.View;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
    }
    
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.Width = 400;
        Application.Current.MainWindow.Height = 600;
    }
    
    private void LoginCommand(object sender, RoutedEventArgs e)
    {
        var mainDataContext = Application.Current.MainWindow.DataContext as MainViewModel;
        if (mainDataContext == null) return;

        mainDataContext.SelectedViewModel = new LoginView();
    }
    
    private void TextBox_UsernameChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            var viewModel = DataContext as RegisterViewModel;
            if (viewModel != null)
            {
                viewModel.UsernameChanged(textBox, e);
            }
        }
    }
    
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = sender as PasswordBox;
        if (passwordBox != null)
        {
            var viewModel = DataContext as RegisterViewModel;
            if (viewModel != null)
            {
                viewModel.PasswordChanged(passwordBox, e);
            }
        }
    }

    private void PasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = sender as PasswordBox;
        if (passwordBox != null)
        {
            var viewModel = DataContext as RegisterViewModel;
            if (viewModel != null)
            {
                viewModel.ConfirmPasswordChanged(passwordBox, e);
            }
        }
    }
}