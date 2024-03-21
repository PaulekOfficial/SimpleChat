using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using SimpleChatProtocol;
using static SimpleChatProtocol.ServerSidePackets;

namespace SimpleChatClient
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly Client _client;

        private IChannelHandlerContext _context;
        private readonly PacketReader _reader;
        private readonly Thread _handshakeTask;
        private readonly Thread _inputThread;
        private readonly IWriter _writer;

        private readonly List<RawPacket> _packets;

        public ClientHandler(Client client)
        {
            this._client = client;
            _packets = new List<RawPacket>();
            _handshakeTask = new Thread(Handle);
            _inputThread = new Thread(InputText);

            this._writer = new PacketWriter();
            _reader = new PacketReader();
        }

        private void Handle()
        {
            var handshake = new Handshake();
            handshake.Port = _client.BindPort;
            handshake.ServerAddress = _client.BindAddress;
            handshake.ProtocolNumber = PacketConstants.ProtocolNumber;
            handshake.Region = "Poland";
            handshake.System = "Windows";
            handshake.NextConnectionState = ConnectionState.PLAY;

            _writer.WritePacket(handshake);
            _writer.Flush(_context);

            var loginRequest = new LoginRequest();
            loginRequest.Username = _client.Nickname;

            _writer.WritePacket(loginRequest);
            _writer.Flush(_context);

            WaitForPacket();
            var encryptionRequest = (EncryptionRequest)_reader.ParsePacket(_packets, new EncryptionRequest());
            _packets.RemoveRange(0, 1);

            var encryptionResponse = new EncryptionResponse();
            encryptionResponse.SharedSecret = CryptoHelper.EncryptString(_client.Certificate2.Issuer, this._client.Certificate2);
            encryptionResponse.SharedToken = CryptoHelper.EncryptString(encryptionRequest.VerifyToken, this._client.Certificate2);

            _writer.WritePacket(encryptionResponse);
            _writer.Flush(_context);

            WaitForPacket();
            var loginSuccess = (LoginSuccess)_reader.ParsePacket(_packets, new LoginSuccess());
            _packets.RemoveRange(0, 1);

            Console.WriteLine($"Podłączono do serwera - id: {loginSuccess.Uuid}");
            Console.WriteLine($"Zalogowano psełdonime - nick: {loginSuccess.Username}");

            // _inputThread.Start();
            // while (true)
            // {
            //     WaitForPacket();
            //     var textPacket = (TextMessagePacket)_reader.ParsePacket(_packets, new TextMessagePacket());
            //     _packets.RemoveRange(0, 1);
            //
            //     Console.WriteLine($"[{textPacket.Username}]: {textPacket.Message}");
            // }
        }
        
        public void SendChatMessage(string message)
        {
            var packet = new TextChatPacket();
            packet.Message = message;

            _writer.WritePacket(packet);
            _writer.Flush(_context);
        }

        private void InputText()
        {
            Console.WriteLine("Wpiszuj wiadomości śmiertelniku!");
            while (true)
            {
                var line = Console.ReadLine();

                var packet = new TextChatPacket();
                packet.Message = line;

                _writer.WritePacket(packet);
                _writer.Flush(_context);
            }
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            var byteBuffer = (IByteBuffer)msg;
            while (byteBuffer.ReadableBytes > 0)
            {
                var raw = _reader.ReadPacket(byteBuffer);
                if (raw == null) continue;

                Console.WriteLine($"Got new packet id: {raw.PacketId()}");

                _packets.Add(raw);
            }
        }

        private void WaitForPacket()
        {
            while (_packets.Count <= 0)
            {
            }
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            _handshakeTask.Interrupt();
            Console.WriteLine($"Closing connection {context.Channel.RemoteAddress}");
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine($"Connection to server active {context.Channel.RemoteAddress}");
            _context = context;
            _handshakeTask.Start();
        }
    }
}
