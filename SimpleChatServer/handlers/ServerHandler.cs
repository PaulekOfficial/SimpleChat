using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using SimpleChatProtocol;

namespace SimpleChatServer;

public class ServerHandler : ChannelHandlerAdapter
{
    private readonly Thread _handshakeTask;

    private readonly List<RawPacket> _packets;

    private readonly Server _server;
    public readonly PacketManager PacketManager;

    public readonly PacketReader Reader;
    public readonly PacketWriter Writer;

    private string _username;
    private Guid _uuid;
    public IChannelHandlerContext Context;

    public ServerHandler(Server server)
    {
        _server = server;
        _handshakeTask = new Thread(Handle);

        _packets = new List<RawPacket>();
        Reader = new PacketReader();
        Writer = new PacketWriter();
        PacketManager = new PacketManager();

        PacketManager.RegisterPacket<TextChatPacket>(0x0D);
        PacketManager.RegisterHandler<TextChatPacket>(0x0D, HandleTextMessagePacket);

        PacketManager.RegisterPacket<FetchContactsPacket>(0x10);
        PacketManager.RegisterHandler<FetchContactsPacket>(0x10, HandleFetchContactsPacket);
        
        PacketManager.RegisterPacket<LoginRequestPacket>(0x11);
        PacketManager.RegisterHandler<LoginRequestPacket>(0x11, HandleLoginRequestPacket);
        
        PacketManager.RegisterPacket<RegisterRequestPacket>(0x12);
        PacketManager.RegisterHandler<RegisterRequestPacket>(0x12, HandleRegisterRequestPacket);
    }

    private void Handle()
    {
        WaitForPacket();
        var handshacke = (Handshake)Reader.ParsePacket(_packets, new Handshake());
        _packets.RemoveRange(0, 1);

        switch (handshacke.NextConnectionState)
        {
            case ConnectionState.PLAY:
                HandlePlay();
                break;
            case ConnectionState.STATUS:
                Console.WriteLine($"Handshake {handshacke.NextConnectionState}");
                break;
            default:
                Console.WriteLine($"Unknown next direction for incoming handshake {handshacke.NextConnectionState}");
                Context.Channel.CloseAsync();
                break;
        }
    }

    public void HandleTextMessagePacket(TextChatPacket packet, EventArgs args)
    {
        var packetToSend = new ServerSidePackets.TextChatMessageHistoryPacket();
        packetToSend.Username = _username;
        packetToSend.Avatar = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg";
        packetToSend.Uuid = _uuid;
        packetToSend.Message = packet.Message;

        _server.SendPacketToAllClients(packetToSend);

        Console.WriteLine($"[{_username}]: {packet.Message}");
    }

    public void HandleFetchContactsPacket(FetchContactsPacket packet, EventArgs args)
    {
        Console.WriteLine($"Fetching contacts for {_username}");

        var contactsPacket = new ServerSidePackets.ClientContactPacket();
        contactsPacket.Uuid = _uuid;
        contactsPacket.Username = _username;
        contactsPacket.Avatar = "https://i.pinimg.com/originals/c8/bd/45/c8bd45cace908c61201a03c53aa502bd.jpg";
        Writer.WritePacket(contactsPacket);
        Writer.Flush(Context);
    }
    
    public void HandleLoginRequestPacket(LoginRequestPacket packet, EventArgs args)
    {
        // if (loginRequest.Username.Length > 25)
        // {
        //     var loginDisconnect = new ServerSidePackets.LoginDisconnect();
        //     loginDisconnect.Reason =
        //         $"Your nickname is to long {loginRequest.Username}, max nick length 25 characters!";
        //
        //     Writer.WritePacket(loginDisconnect);
        //     Writer.Flush(Context);
        //     Context.Channel.CloseAsync();
        //     return;
        // }
        
        // _username = loginRequest.Username;
        // _uuid = Guid.NewGuid();
        //
        // var loginSuccess = new ServerSidePackets.LoginSuccess();
        // loginSuccess.Username = _username;
        // loginSuccess.Uuid = _uuid;
        //
        // Writer.WritePacket(loginSuccess);
        // Writer.Flush(Context);
        //
        // var client = new Client(_uuid, _username, this);
        // _server.Clients.Add(_uuid, client);
    }
    
    public void HandleRegisterRequestPacket(RegisterRequestPacket packet, EventArgs args)
    {

    }

    //TODO load user data from databse
    private void HandlePlay()
    {
        WaitForPacket();
        var loginRequest = (LoginStartRequest)Reader.ParsePacket(_packets, new LoginStartRequest());
        _packets.RemoveRange(0, 1);

        Console.WriteLine(
            $"User is beginning login to server. {Context.Channel.RemoteAddress}");

        var token = Randomizer.String(16);
        var encryptionRequest = new ServerSidePackets.EncryptionRequest();
        encryptionRequest.EncryptionProtocol = CryptoHelper.SIGNATURE_ALGORITHM;
        encryptionRequest.PublicKey = _server.Certificate2.PublicKey.EncodedKeyValue.RawData;
        encryptionRequest.VerifyToken = token;
        encryptionRequest.ServerId = _server.ServerId;

        Writer.WritePacket(encryptionRequest);
        Writer.Flush(Context);

        WaitForPacket();
        var encryptionResponse = (EncryptionResponse)Reader.ParsePacket(_packets, new EncryptionResponse());
        _packets.RemoveRange(0, 1);
        if (token != CryptoHelper.DecryptString(encryptionResponse.SharedToken, _server.Certificate2))
        {
            var loginDisconnect = new ServerSidePackets.LoginDisconnect();
            loginDisconnect.Reason = "Cert error: Invalid shared token!";

            Writer.WritePacket(loginDisconnect);
            Writer.Flush(Context);
            Context.Channel.CloseAsync();
            return;
        }

        if (_server.Certificate2.Issuer !=
            CryptoHelper.DecryptString(encryptionResponse.SharedSecret, _server.Certificate2))
        {
            var loginDisconnect = new ServerSidePackets.LoginDisconnect();
            loginDisconnect.Reason = "Cert error: Invalid shared secret!";

            Writer.WritePacket(loginDisconnect);
            Writer.Flush(Context);
            Context.Channel.CloseAsync();
            return;
        }
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        var byteBuffer = (IByteBuffer)msg;
        while (byteBuffer.ReadableBytes > 0)
        {
            var raw = Reader.ReadPacket(byteBuffer);
            if (raw == null) continue;

            _packets.Add(raw);
            PacketManager.CallAndForget(raw, new EventArgs());
        }
    }

    private void WaitForPacket()
    {
        while (_packets.Count <= 0)
        {
        }
    }

    #region Channel Register/Unregister

    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine($"Client channel activated {context.Channel.RemoteAddress}");
        Context = context;
        _handshakeTask.Start();
    }

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
        Console.WriteLine($"Initial connection to client {context.Channel.RemoteAddress}");
    }

    public override void ChannelUnregistered(IChannelHandlerContext context)
    {
        _handshakeTask.Interrupt();
        Console.WriteLine($"Closing connection to client {context.Channel.RemoteAddress}");
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine($"An exception occurred at handler {context.Channel.RemoteAddress}, {exception}");
    }

    #endregion
}