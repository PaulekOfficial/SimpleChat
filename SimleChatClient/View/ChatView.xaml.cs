using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Models.Model;
using ChatClientGUI.Models.ViewModel;
using SimpleChatClient;

namespace ChatClientGUI.View;

public partial class ChatView : UserControl
{
    private readonly Client _client = ClientFactory.GetClientInstance();

    public ChatView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.Width = 1200;
        Application.Current.MainWindow.Height = 650;
    }

    private void UIContact_OnElementChange(object sender, SelectionChangedEventArgs e)
    {
        var viewModel = DataContext as ChatViewModel;
        if (viewModel == null) return;

        var listBox = sender as ListBox;
        if (listBox == null || listBox.SelectedItem == null) return;

        viewModel.ActiveContactModel = listBox.SelectedItem as ContactModel;
    }

    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var viewModel = DataContext as ChatViewModel;
        if (viewModel == null) return;
        _client.ChannelHandler.SendChatMessage(viewModel.Message);
        viewModel.Message = string.Empty;
        viewModel.SendCommand();

        var textBox = sender as TextBox;
        if (textBox == null || string.IsNullOrEmpty(textBox.Text)) return;
    }
}