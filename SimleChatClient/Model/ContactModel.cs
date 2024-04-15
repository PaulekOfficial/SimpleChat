using System.Collections.ObjectModel;
using ChatClientGUI.Core;

namespace ChatClientGUI.Models.Model;

public class ContactModel : ObservableObject
{
    public string? _lastMessage;

    public ContactModel()
    {
        Uuid = Guid.Empty;
        Username = "";
        Avatar = "";
        Messages = new ObservableCollection<MessageModel>();
    }

    public ContactModel(Guid uuid, string username, string avatar)
    {
        Uuid = uuid;
        Username = username;
        Avatar = avatar;
        Messages = new ObservableCollection<MessageModel>();
    }

    public Guid Uuid { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public ObservableCollection<MessageModel> Messages { get; set; }


    public string LastMessage
    {
        get => _lastMessage;
        set
        {
            _lastMessage = value;
            OnPropertyChanged();
        }
    }
}