namespace SimpleChatServer;

public class ChatGroup
{
    public ChatGroup(string name, string avatar, bool privateGroup)
    {
        GroupId = Guid.NewGuid();
        Name = name;
        Avatar = avatar;
        this.privateGroup = privateGroup;
        createdAt = DateTime.Now;
    }

    public ChatGroup(int id, Guid groupId, string avatar, string name, bool privateGroup, DateTime createdAt)
    {
        Id = id;
        GroupId = groupId;
        Avatar = avatar;
        Name = name;
        this.privateGroup = privateGroup;
        this.createdAt = createdAt;
    }

    public int Id { get; set; }
    public Guid GroupId { get; }
    public List<Client> Users { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
    public string Avatar { get; set; }
    public string Name { get; set; }
    public bool privateGroup { get; set; }
    public DateTime createdAt { get; }

    public void AddUser(Client user)
    {
        Users.Add(user);
    }

    public void RemoveUser(Client user)
    {
        Users.Remove(user);
    }

    public void AddMessage(ChatMessage message)
    {
        Messages.Add(message);
    }

    public void RemoveMessage(ChatMessage message)
    {
        Messages.Remove(message);
    }

    public void SetAvatar(string avatar)
    {
        Avatar = avatar;
    }
}