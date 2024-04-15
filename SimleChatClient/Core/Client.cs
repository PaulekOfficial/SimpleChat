using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using SimpleChatProtocol;

namespace SimpleChatClient;

public class Client
{
    public IChannel BootstrapChannel;
    public MultithreadEventLoopGroup Group = new();

    public Client(string nickname, string bindAddress, int port)
    {
        Certificate2 = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");

        Nickname = nickname;
        BindAddress = bindAddress;
        BindPort = port;

        ChannelHandler = new ClientHandler(this);
    }

    public ClientHandler ChannelHandler { get; set; }
    public string BindAddress { get; }
    public int BindPort { get; }
    public string Nickname { get; set; }
    public Task NetworkTask { get; set; }
    public X509Certificate2 Certificate2 { get; set; }

    public void SendPacket(IPacket packet)
    {
        ChannelHandler.SendPacket(packet);
    }

    public async Task RunClientAsync()
    {
        try
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(Group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;

                    pipeline.AddLast(ChannelHandler);
                    pipeline.AddLast(
                        new TlsHandler(
                            stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true),
                            new ClientTlsSettings(Certificate2.GetNameInfo(X509NameType.DnsName, false)))
                    );
                }));

            BootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(BindAddress), BindPort));

            for (;;)
            {
                //DUMMY AKAPAKA
            }

            await BootstrapChannel.CloseAsync();
        }
        finally
        {
            Group.ShutdownGracefullyAsync().Wait(1000);
        }
    }
}