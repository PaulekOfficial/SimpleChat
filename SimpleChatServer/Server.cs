using System.Net;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using SimpleChatProtocol;
using SimpleChatServer.cache;

namespace SimpleChatServer;

public class Server
{
    private readonly ClientCache _clientCache;
    private readonly ICache<Guid, ChatGroup> _groupCache;
    private readonly ICache<Guid, ChatMessage> _messageCache;

    //TODO: Implement multiserver support
    public string DATABASE_CONNECTION_STRING =
        "Server=192.168.3.241;Database=chat;User=superpaulek;Password=Yrhzmudg@1;";

    public Server()
    {
        ServerId = Guid.NewGuid();

        _clientCache = new ClientCache(this);
        _clientCache.Initialize();

        _messageCache = new ChatMessageCache(this);
        _messageCache.Initialize();

        _groupCache = new ChatGroupCache(this);
        _groupCache.Initialize();

        Certificate2 = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");
    }
    
    public Dictionary<Guid, Client> Clients { get; set; } = new();

    public X509Certificate2 Certificate2 { get; set; }

    public Guid ServerId { get; set; }

    public string BindAddress { get; set; } = "127.0.0.1";

    public int BindPort { get; set; } = 9082;

    public void RunServer()
    {
        RunServerAsync().Wait();
    }

    public void SendPacketToAllClients(IPacket packet)
    {
        foreach (var client in Clients.Values)
        {
            client.Handler.Writer.WritePacket(packet);
            client.Handler.Writer.Flush(client.Handler.Context);
        }
    }

    public void SendPacketToAllClients(IPacket packet, params Guid[] userBlacklist)
    {
        foreach (var client in Clients.Values)
        {
            if (userBlacklist.Contains(client.Uuid)) continue;

            client.Handler.Writer.WritePacket(packet);
            client.Handler.Writer.Flush(client.Handler.Context);
        }
    }

    public async Task RunServerAsync()
    {
        var mainThreadsGroup = new MultithreadEventLoopGroup(1);
        var workersThreadsGroup = new MultithreadEventLoopGroup();

        var bootstrap = new ServerBootstrap();
        try
        {
            Console.WriteLine("Starting server...");

            bootstrap
                .Group(mainThreadsGroup, workersThreadsGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 500)
                .Handler(new LoggingHandler(LogLevel.INFO))
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;

                    pipeline.AddLast(new ServerHandler(this));
                    pipeline.AddLast(TlsHandler.Server(Certificate2));
                }));

            Console.WriteLine($"Validate server core: {bootstrap.Validate()}");
            Console.WriteLine($"Binding host: {BindAddress} at port {BindPort}");

            var bootstrapChannel = await bootstrap.BindAsync(IPAddress.Parse(BindAddress), BindPort);

            Console.ReadLine();

            await bootstrapChannel.CloseAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Could not start/bind server, {exception}");
        }
        finally
        {
            Task.WaitAll(mainThreadsGroup.ShutdownGracefullyAsync(), workersThreadsGroup.ShutdownGracefullyAsync());
        }
    }
    
    public Client? GetClientByUsername(String username)
    {
        return _clientCache.Get(username);
    }
    
    public Client? GetClientByUuid(Guid uuid)
    {
        return _clientCache.Get(uuid);
    }
    
    public ClientCache GetClientCache()
    {
        return _clientCache;
    }
}