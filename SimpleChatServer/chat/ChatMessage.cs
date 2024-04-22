namespace SimpleChatServer;

public class ChatMessage
{
    public ChatMessage()
    {
    }
    
    public ChatMessage(Guid uuid, Guid groupUuid, Guid senderUuid, string content, DateTime sentAt)
    {
        Uuid = uuid;
        GroupUuid = groupUuid;
        SenderUuid = senderUuid;
        Content = content;
        SentAt = sentAt;
    }
    
    public ChatMessage(int id, Guid uuid, Guid groupUuid, Guid senderUuid, string content, DateTime sentAt)
    {
        Id = id;
        Uuid = uuid;
        GroupUuid = groupUuid;
        SenderUuid = senderUuid;
        Content = content;
        SentAt = sentAt;
    }

    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public Guid GroupUuid { get; set; }
    public Guid SenderUuid { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
}