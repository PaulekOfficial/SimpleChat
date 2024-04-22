using System.Windows;
using System.Windows.Controls;
using ChatClientGUI.Models.ViewModel;
using SimleChatClient.View;

namespace ChatClientGUI.View;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
    
    private void TextBox_UsernameChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            var viewModel = DataContext as LoginViewModel;
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
            var viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.PasswordChanged(passwordBox, e);
            }
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.Width = 400;
        Application.Current.MainWindow.Height = 600;
    }
    
    private void RegisterCommand(object sender, RoutedEventArgs e)
    {
        var mainDataContext = Application.Current.MainWindow.DataContext as MainViewModel;
        if (mainDataContext == null) return;

        mainDataContext.SelectedViewModel = new RegisterView();
    }

    private void LoginCommand(object sender, RoutedEventArgs e)
    {
        var mainDataContext = Application.Current.MainWindow.DataContext as MainViewModel;
        if (mainDataContext == null) return;

        mainDataContext.SelectedViewModel = new ChatView();
    }
}