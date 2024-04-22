using Dapper;
using MySqlConnector;

namespace SimpleChatServer.cache;

public class ChatMessageCache : ICache<Guid, ChatMessage>
{
    private readonly Server _server;
    private readonly Dictionary<Guid, ChatMessage> _cache;

    public ChatMessageCache(Server server)
    {
        _server = server;
        _cache = new Dictionary<Guid, ChatMessage>();
    }

    public bool Initialize()
    {
        return CreateTable();
    }

    public ChatMessage? Get(Guid key)
    {
        if (_cache.ContainsKey(key)) return _cache[key];

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var message = connection.QueryFirstOrDefault<ChatMessage>("SELECT * FROM chat_message WHERE Uuid = @Uuid",
                new { Uuid = key });
            if (message != null) _cache.Add(key, message);

            return message;
        }
    }

    public bool Add(Guid key, ChatMessage value)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute(
                "INSERT INTO chat_message (Uuid, SenderUuid, GroupUuid, Content, SentAt) VALUES (@Uuid, @SenderUuid, @GroupUuid, @Content, @SentAt)",
                value);
            if (affectedRows > 0) _cache.Add(key, value);

            return affectedRows > 0;
        }
    }

    public bool Save(ChatMessage value)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute(
                "UPDATE chat_message SET SenderUuid = @SenderUuid, GroupUuid = @GroupUuid, Content = @Content, SentAt = @SentAt WHERE Uuid = @Uuid",
                value);
            return affectedRows > 0;
        }
    }

    public bool Remove(Guid key)
    {
        if (_cache.ContainsKey(key)) _cache.Remove(key);

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute("DELETE FROM chat_message WHERE Uuid = @Uuid", new { Uuid = key });
            return affectedRows > 0;
        }
    }

    public bool Contains(Guid key)
    {
        if (_cache.ContainsKey(key)) return true;

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var message = connection.QueryFirstOrDefault<ChatMessage>("SELECT * FROM chat_message WHERE Uuid = @Uuid",
                new { Uuid = key });
            if (message != null) _cache.Add(key, message);

            return message != null;
        }
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    public List<ChatMessage> GetFirstMessages(ChatGroup group, int limit)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection
                .Query<ChatMessage>(
                    "SELECT * FROM chat_message WHERE GroupUuid = @GroupUuid ORDER BY SentAt LIMIT " + limit,
                    new { GroupId = group.GroupId }).ToList();
        }
    }

    public List<ChatMessage> GetMessagesFromPeriod(Guid group, DateTime start, DateTime end)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection
                .Query<ChatMessage>(
                    "SELECT * FROM chat_message WHERE GroupUuid = @GroupId AND SentAt BETWEEN @Start AND @End",
                    new { GroupId = group.ToString(), Start = start, End = end }).ToList();
        }
    }

    private bool CreateTable()
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute(
                "CREATE TABLE IF NOT EXISTS chat_message (Id INT PRIMARY KEY AUTO_INCREMENT, Uuid CHAR(36), SenderUuid CHAR(36), GroupUuid CHAR(36), Content TEXT, SentAt DATETIME)");
            return affectedRows > 0;
        }
    }
}