using Dapper;
using MySqlConnector;

namespace SimpleChatServer.cache;

public class ClientCache : ICache<Guid, Client>
{
    private readonly Server _server;
    private readonly Dictionary<Guid, Client> _clients;

    public ClientCache(Server server)
    {
        _server = server;
        _clients = new Dictionary<Guid, Client>();
    }

    public bool Initialize()
    {
        return CreateClientTable();
    }
    
    public Client? Get(String username)
    {
        foreach (var clientPair in _clients)
        {
            if (clientPair.Value.Username.ToLower() == username.ToLower())
            {
                return clientPair.Value;
            }
        }

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var client =
                connection.QueryFirstOrDefault<Client>("SELECT * FROM client WHERE Username = @Username", new { Username = username });
            if (client != null) _clients.Add(client.Uuid, client);

            return client;
        }
    }

    public Client? Get(Guid key)
    {
        if (_clients.ContainsKey(key)) return _clients[key];

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var client =
                connection.QueryFirstOrDefault<Client>("SELECT * FROM client WHERE Uuid = @Uuid", new { Uuid = key });
            if (client != null) _clients.Add(key, client);

            return client;
        }
    }

    public bool Add(Guid key, Client value)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows =
                connection.Execute(
                    "INSERT INTO client (Uuid, Username, Password, Avatar, Status, ChatStatus, JoinedAt) VALUES (@Uuid, @Username, @Password, @Avatar, @Status, @ChatStatus, @JoinedAt)",
                    value);
            if (affectedRows > 0) _clients.Add(key, value);

            return affectedRows > 0;
        }
    }

    public bool Save(Client value)
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows =
                connection.Execute(
                    "UPDATE client SET Username = @Username, Password = @Password, Avatar = @Avatar, Status = @Status, ChatStatus = @ChatStatus, JoinedAt = @JoinedAt WHERE Uuid = @Uuid",
                    value);
            return affectedRows > 0;
        }
    }

    public bool Remove(Guid key)
    {
        if (_clients.ContainsKey(key)) _clients.Remove(key);

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute("DELETE FROM client WHERE Uuid = @Uuid", new { Uuid = key });
            return affectedRows > 0;
        }
    }

    public bool Contains(Guid key)
    {
        if (_clients.ContainsKey(key)) return true;

        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            return connection.Query<Client>("SELECT * FROM client WHERE Uuid = @Uuid", new { Uuid = key }).Any();
        }
    }

    public void ClearCache()
    {
        _clients.Clear();
    }

    private bool CreateClientTable()
    {
        using (var connection = new MySqlConnection(_server.DATABASE_CONNECTION_STRING))
        {
            var affectedRows = connection.Execute(
                "CREATE TABLE IF NOT EXISTS client (Id INT AUTO_INCREMENT PRIMARY KEY, Uuid CHAR(36) NOT NULL UNIQUE, Username VARCHAR(50) NOT NULL, Password TEXT, Avatar VARCHAR(255), Status VARCHAR(50), ChatStatus INT NOT NULL, JoinedAt DATETIME NOT NULL)");
            return affectedRows > 0;
        }
    }
}