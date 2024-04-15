namespace SimpleChatServer;

public class ChatMessage
{
    public ChatMessage(int id, Guid messageId, Guid groupId, Guid senderId, string content, DateTime sentAt)
    {
        Id = id;
        MessageId = messageId;
        GroupId = groupId;
        SenderId = senderId;
        Content = content;
        SentAt = sentAt;
    }

    public int Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid GroupId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
}