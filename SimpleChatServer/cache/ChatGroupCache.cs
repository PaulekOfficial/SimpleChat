using Dapper;
using MySqlConnector;

namespace SimpleChatServer.cache;

public class ChatGroupCache : ICache<Guid, ChatGroup>
{
    private readonly Server _server;
    private readonly Dictionary<Guid, ChatGroup> _cache;

    public ChatGroupCache(Server server)
    {
        _server = server;
        _cache = new Dictionary<Guid, ChatGroup>();
    }

    public bool Initialize()
    {
        return CreateTable();
    }

    public ChatGroup? Get(Guid key)
    {
        if (_cache.ContainsKey(key)) return _cache[key];

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.QueryFirstOrDefault<ChatGroup>("SELECT * FROM chat_group WHERE GroupId = @GroupId",
                new { GroupId = key });
        }
    }

    public bool Add(Guid key, ChatGroup value)
    {
        if (_cache.ContainsKey(key)) return false;

        _cache.Add(key, value);

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.Execute(
                "INSERT INTO chat_group (GroupId, Avatar, Name, PrivateGroup, CreatedAt) VALUES (@GroupId, @Avatar, @Name, @PrivateGroup, @CreatedAt)",
                new
                {
                    value.GroupId, value.Avatar, value.Name, PrivateGroup = value.privateGroup,
                    CreatedAt = value.createdAt
                }) > 0;
        }
    }

    public bool Save(ChatGroup value)
    {
        if (!_cache.ContainsKey(value.GroupId)) return false;

        _cache[value.GroupId] = value;

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.Execute(
                "UPDATE chat_group SET Avatar = @Avatar, Name = @Name, PrivateGroup = @PrivateGroup WHERE GroupId = @GroupId",
                new { value.Avatar, value.Name, PrivateGroup = value.privateGroup, value.GroupId }) > 0;
        }
    }

    public bool Remove(Guid key)
    {
        if (!_cache.ContainsKey(key)) _cache.Remove(key);

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.Execute("DELETE FROM chat_group WHERE GroupId = @GroupId", new { GroupId = key }) > 0;
        }
    }

    public bool Contains(Guid key)
    {
        if (_cache.ContainsKey(key)) return true;

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.QueryFirstOrDefault<ChatGroup>("SELECT * FROM chat_group WHERE GroupId = @GroupId",
                new { GroupId = key }) != null;
        }
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    private bool CreateTable()
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows0 = connection.Execute(
                "CREATE TABLE IF NOT EXISTS chat_group (Id INT PRIMARY KEY AUTO_INCREMENT, GroupId CHAR(36) NOT NULL, Avatar TEXT NOT NULL, Name TEXT NOT NULL, PrivateGroup BOOLEAN NOT NULL, CreatedAt DATETIME NOT NULL)");

            var affectedRows1 =
                connection.Execute(
                    "CREATE TABLE IF NOT EXISTS chat_group_users (Id INT PRIMARY KEY AUTO_INCREMENT, GroupId CHAR(36) NOT NULL, UserId CHAR(36) NOT NULL)");

            return affectedRows1 > 0 && affectedRows0 > 0;
        }
    }
}