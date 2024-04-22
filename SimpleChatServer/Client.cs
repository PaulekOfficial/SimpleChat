using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleChatServer;

public class Client
{
    public Client()
    {
    }

    public Client(Guid uuid, string username, string password, ServerHandler handler)
    {
        Uuid = uuid;
        Handler = handler;
        Username = username;
        Password = password;
        Status = "Online";
        ChatStatus = ChatStatus.Active;
        JoinedAt = DateTime.Now;
    }

    public Client(Guid uuid, string username, string avatar, string status, ChatStatus chatStatus, DateTime joinedAt,
        ServerHandler handler)
    {
        Uuid = uuid;
        Username = username;
        Avatar = avatar;
        Status = status;
        ChatStatus = chatStatus;
        JoinedAt = joinedAt;
        Handler = handler;
    }

    public Client(int id, Guid uuid, string username, string password, string avatar, string status,
        ChatStatus chatStatus, DateTime joinedAt, ServerHandler handler)
    {
        Id = id;
        Uuid = uuid;
        Username = username;
        Password = password;
        Avatar = avatar;
        Status = status;
        ChatStatus = chatStatus;
        JoinedAt = joinedAt;
        Handler = handler;
    }

    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Avatar { get; set; }
    public string Status { get; set; }
    public ChatStatus ChatStatus { get; set; }
    public DateTime JoinedAt { get; set; }

    [NotMapped] public ServerHandler Handler { get; set; }

    public void EncryptPassword()
    {
        Password = BCrypt.Net.BCrypt.HashPassword(Password);
    }

    public override string ToString()
    {
        return
            $"Client{{Uuid={Uuid}, Username={Username}, Password={Password} Avatar={Avatar}, Status={Status}, ChatStatus={ChatStatus}, JoinedAt={JoinedAt}, Handler={Handler}}}";
    }
}