using System.Collections.ObjectModel;
using System.Windows;
using ChatClientGUI.Core;
using ChatClientGUI.Models.Model;
using SimpleChatClient;
using SimpleChatProtocol;

namespace ChatClientGUI.Models.ViewModel;

public class ChatViewModel : ObservableObject
{
    private Client _client = ClientFactory.GetClientInstance();

    public string Nickname { get; set; }
    
    public string Message { get; set; }

    public ObservableCollection<MessageModel> Messages { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }
    
    public ChatViewModel()
    {
        Nickname = _client.Nickname;
        
        Messages = new ObservableCollection<MessageModel>();
        Contacts = new ObservableCollection<ContactModel>();
        
        _client.ChannelHandler.GetPacketManager().RegisterHandler<ServerSidePackets.TextChatMessageHistoryPacket>(0x0F, HandleTextMessagePacket);
    }
    
    public void HandleTextMessagePacket(ServerSidePackets.TextChatMessageHistoryPacket historyPacket, EventArgs eventArgs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(new MessageModel
            {
                Username = historyPacket.Username,
                Message = historyPacket.Message,
                UsernameColor = "#409aff",
                ImageMessage = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true
            });
        });
    }
    
    ~ChatViewModel()
    {
        _client.ChannelHandler.GetPacketManager().UnregisterHandler(0x0F);
    }
    
    public void SendCommand()
    {
       OnPropertyChanged(nameof(Message));
    }
}