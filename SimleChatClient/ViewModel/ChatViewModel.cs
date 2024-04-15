using System.Collections.ObjectModel;
using System.Windows;
using ChatClientGUI.Core;
using ChatClientGUI.Models.Model;
using SimpleChatClient;
using SimpleChatProtocol;

namespace ChatClientGUI.Models.ViewModel;

public class ChatViewModel : ObservableObject
{
    private ContactModel? _activeContactModel;
    private readonly Client _client = ClientFactory.GetClientInstance();

    public ChatViewModel()
    {
        Nickname = _client.Nickname;
        Contacts = new ObservableCollection<ContactModel>();
        ActiveContactModel = new ContactModel(
            Guid.NewGuid(),
            "Test",
            "https://www.gravatar.com/avatar/205e460b479e2e5b48aec07710c08d50");


        _client.ChannelHandler.GetPacketManager()
            .RegisterHandler<ServerSidePackets.TextChatMessageHistoryPacket>(0x0F, HandleTextMessagePacket);
        _client.ChannelHandler.GetPacketManager()
            .RegisterHandler<ServerSidePackets.ClientContactPacket>(0x1A, HandleClientContactPacket);
    }

    public string Nickname { get; set; }
    public string Message { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }

    public ContactModel? ActiveContactModel
    {
        get => _activeContactModel;
        set
        {
            _activeContactModel = value;
            OnPropertyChanged();
        }
    }

    public void HandleTextMessagePacket(ServerSidePackets.TextChatMessageHistoryPacket historyPacket,
        EventArgs eventArgs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var contact in Contacts)
            {
                if (contact.Uuid != historyPacket.Uuid) continue;

                contact.Messages.Add(new MessageModel
                {
                    Username = historyPacket.Username,
                    Message = historyPacket.Message,
                    UsernameColor = "#409aff",
                    ImageMessage = historyPacket.Avatar,
                    Time = historyPacket.Time,
                    IsNativeOrigin = true,
                    FirstMessage = contact.Messages.Count == 0 ||
                                   contact.Messages.Last().Username != historyPacket.Username
                });
                OnPropertyChanged(nameof(contact.Messages));

                contact.LastMessage = historyPacket.Message;
            }
        });
    }

    public void HandleClientContactPacket(ServerSidePackets.ClientContactPacket packet, EventArgs eventArgs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Contacts.Add(new ContactModel
            {
                Uuid = packet.Uuid,
                Username = packet.Username,
                Avatar = packet.Avatar
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