using System.IO;
using DotNetty.Transport.Bootstrapping;
using System.Net;
using System.Net.Security;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Handlers.Tls;

namespace SimpleChatClient
{
    public class Client
    {
        public MultithreadEventLoopGroup Group = new MultithreadEventLoopGroup();
        public IChannel BootstrapChannel;

        public string BindAddress { get; }
        public int BindPort { get; }

        public string Nickname { get; set; }
        
        public Task NetworkTask {get; set;}

        public Client(string nickname, string bindAddress, int port)
        {
            Certificate2 = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");

            Nickname = nickname;
            BindAddress = bindAddress;
            BindPort = port;
        }

        public X509Certificate2 Certificate2 { get; set; }

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
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new ClientHandler(this));
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
}
