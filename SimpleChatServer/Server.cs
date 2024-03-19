using System;
using System.Collections.Generic;
using System.IO;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Handlers.Tls;
using SimpleChatProtocol;

namespace SimpleChatServer
{
    public class Server
    {
        public string bindAddress = "127.0.0.1";
        public int bindPort = 9082;

        public X509Certificate2 Certificate2 { get; set; }

        public Dictionary<Guid, Client> Clients { get; set; } = new();

        public Guid ServerId { get; set; }

        public Server()
        {
            ServerId = Guid.NewGuid();

            Certificate2 = new X509Certificate2(Path.Combine("", "dotnetty.com.pfx"), "password");
        }

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
                if (userBlacklist.Contains(client.Uuid))
                {
                    continue;
                }

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
                Console.WriteLine($"Binding host: {bindAddress} at port {bindPort}");

                var bootstrapChannel = await bootstrap.BindAsync(IPAddress.Parse(bindAddress), bindPort);

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
    }
}
