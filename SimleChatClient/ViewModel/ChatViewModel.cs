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
    
    public string _nickname;
    public string _status;
    public string _avatar;
    public string _chatWith;
    public string Message { get; set; }
    public ObservableCollection<ContactModel> Contacts { get; set; }

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
        
        _client.ChannelHandler.GetPacketManager()
            .RegisterHandler<ServerSidePackets.ClientStatusPacket>(0x1D, HandleClientStatusPacket);
        
        _client.FetchContacts();
    }

    public void HandleClientStatusPacket(ServerSidePackets.ClientStatusPacket packet, EventArgs eventArgs)
    {
        Avatar = packet.Avatar;
        Status = packet.Status;
    }

    public void HandleTextMessagePacket(ServerSidePackets.TextChatMessageHistoryPacket historyPacket,
        EventArgs eventArgs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var contact in Contacts)
            {
                if (contact.Uuid != historyPacket.GroupId) continue;

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
    
    public string Nickname
    {
        get => _nickname;
        set
        {
            _nickname = value;
            OnPropertyChanged(nameof(Nickname));
        }
    }
    
    public ContactModel? ActiveContactModel
    {
        get => _activeContactModel;
        set
        {
            _activeContactModel = value;
            OnPropertyChanged();
            
            if (value == null) return;
            ChatWith = value.Username;
        }
    }
    
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }
    
    public string Avatar
    {
        get => _avatar;
        set
        {
            _avatar = value;
            OnPropertyChanged(nameof(Avatar));
        }
    }
    
    public string ChatWith
    {
        get => _chatWith;
        set
        {
            _chatWith = "Chat with " + value;
            OnPropertyChanged(nameof(ChatWith));
        }
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