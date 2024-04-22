using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Models.Model;
using ChatClientGUI.Models.ViewModel;
using SimpleChatClient;
using SimpleChatProtocol;

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

        var fetchMessages = new FetchGroupMessagesPacket();
        fetchMessages.GroupId = viewModel.ActiveContactModel.Uuid;
        fetchMessages.UserId = _client.Uuid;
        
        _client.ChannelHandler.SendPacket(fetchMessages);
    }

    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var viewModel = DataContext as ChatViewModel;
        if (viewModel == null) return;
        if (viewModel.ActiveContactModel == null) return;
        _client.ChannelHandler.SendChatMessage(viewModel.Message, viewModel.ActiveContactModel.Uuid);
        viewModel.Message = string.Empty;
        viewModel.SendCommand();

        var textBox = sender as TextBox;
        if (textBox == null || string.IsNullOrEmpty(textBox.Text)) return;
    }
}