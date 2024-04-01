using System.Windows.Controls;
using System.Windows.Input;
using ChatClientGUI.Models.ViewModel;
using SimpleChatClient;

namespace ChatClientGUI.View;

public partial class ChatView : UserControl
{
    private Client _client = ClientFactory.GetClientInstance();
    
    public ChatView()
    {
        InitializeComponent();
    }

    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }
        
        var viewModel = DataContext as ChatViewModel;
        if(viewModel == null)
        {
            return;
        }
        _client.ChannelHandler.SendChatMessage(viewModel.Message);
        viewModel.Message = string.Empty;
        viewModel.SendCommand();
        
        var textBox = sender as TextBox;
        if(textBox == null || string.IsNullOrEmpty(textBox.Text))
        {
            return;
        }
    }
}