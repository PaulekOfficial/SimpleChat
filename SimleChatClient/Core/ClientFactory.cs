namespace SimpleChatClient;

public class ClientFactory
{
    private static readonly Client Client;

    static ClientFactory()
    {
        Client = new Client("PaulekOfficial", "127.0.0.1", 9082);

        Client.NetworkTask = new Task(() => { Client.RunClientAsync().Wait(); });

        Client.NetworkTask.Start();
    }

    public static Client GetClientInstance()
    {
        return Client;
    }
}