using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using SimpleChatProtocol;
using static SimpleChatProtocol.ServerSidePackets;

namespace SimpleChatClient;

public class ClientHandler : ChannelHandlerAdapter
{
    private readonly Client _client;
    private readonly Thread _handshakeTask;
    private readonly Thread _inputThread;
    private readonly PacketManager _packetManager;

    private readonly List<RawPacket> _packets;
    private readonly PacketReader _reader;
    private readonly IWriter _writer;

    private IChannelHandlerContext _context;

    public ClientHandler(Client client)
    {
        _client = client;
        _packets = new List<RawPacket>();
        _handshakeTask = new Thread(Handle);

        _writer = new PacketWriter();
        _reader = new PacketReader();

        _packetManager = new PacketManager();
        _packetManager.RegisterPacket<TextChatMessageHistoryPacket>(0x0F);
        _packetManager.RegisterPacket<ClientContactPacket>(0x1A);
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

        var loginRequest = new LoginStartRequest();
        //loginRequest.Username = _client.Nickname;

        _writer.WritePacket(loginRequest);
        _writer.Flush(_context);

        WaitForPacket();
        var encryptionRequest = (EncryptionRequest)_reader.ParsePacket(_packets, new EncryptionRequest());
        _packets.RemoveRange(0, 1);

        var encryptionResponse = new EncryptionResponse();
        encryptionResponse.SharedSecret = CryptoHelper.EncryptString(_client.Certificate2.Issuer, _client.Certificate2);
        encryptionResponse.SharedToken =
            CryptoHelper.EncryptString(encryptionRequest.VerifyToken, _client.Certificate2);

        _writer.WritePacket(encryptionResponse);
        _writer.Flush(_context);

        WaitForPacket();
        var loginSuccess = (LoginSuccess)_reader.ParsePacket(_packets, new LoginSuccess());
        _packets.RemoveRange(0, 1);

        Console.WriteLine($"Podłączono do serwera - id: {loginSuccess.Uuid}");
        Console.WriteLine($"Zalogowano username - nick: {loginSuccess.Username}");


        // Send a request to the server to get the contacts
        var contactsPacket = new FetchContactsPacket();
        contactsPacket.Uuid = loginSuccess.Uuid;
        SendPacket(contactsPacket);
    }

    public void SendChatMessage(string message)
    {
        var packet = new TextChatPacket();
        packet.Message = message;

        _writer.WritePacket(packet);
        _writer.Flush(_context);
    }

    public void SendPacket(IPacket packet)
    {
        _writer.WritePacket(packet);
        _writer.Flush(_context);
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        var byteBuffer = (IByteBuffer)msg;
        while (byteBuffer.ReadableBytes > 0)
        {
            var raw = _reader.ReadPacket(byteBuffer);
            if (raw == null) continue;

            _packetManager.CallAndForget(raw, new EventArgs());
            _packets.Add(raw);
        }
    }

    private void WaitForPacket()
    {
        while (_packets.Count <= 0)
        {
        }
    }

    public PacketManager GetPacketManager()
    {
        return _packetManager;
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