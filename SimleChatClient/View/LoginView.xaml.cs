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