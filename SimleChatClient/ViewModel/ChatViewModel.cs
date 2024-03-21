using System.Collections.ObjectModel;
using ChatClientGUI.Core;
using ChatClientGUI.Models.Model;
using SimpleChatClient;

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
        
        Messages.Add(new MessageModel
        {
            Username = "PaulekOfficial",
            UsernameColor = "#409aff",
            ImageMessage = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg",
            Message = "Test notyfikacji push ęążźćńłóęąś",
            Time = DateTime.Now,
            IsNativeOrigin = true,
            FirstMessage = true
        });
        
        Messages.Add(new MessageModel
        {
            Username = "PaulekOfficial",
            UsernameColor = "#409aff",
            ImageMessage = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg",
            Message = "MBank pozdrawia.",
            Time = DateTime.Now,
            IsNativeOrigin = true,
            FirstMessage = true
        });
        
        Messages.Add(new MessageModel
        {
            Username = "PaulekOfficial",
            UsernameColor = "#409aff",
            ImageMessage = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg",
            Message = "Test",
            Time = DateTime.Now,
            IsNativeOrigin = true,
            FirstMessage = true
        });
        
        Contacts.Add(new ContactModel
        {
            Username = "PaulekOfficial",
            ImageSource = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg",
            Messages = Messages
        });
    }
    
    public void SendCommand()
    {
        Console.WriteLine(Message);
    }
}