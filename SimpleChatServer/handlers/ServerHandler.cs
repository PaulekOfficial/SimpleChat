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
        
        PacketManager.RegisterPacket<UsernameCheckRequestPacket>(0x13);
        PacketManager.RegisterHandler<UsernameCheckRequestPacket>(0x13, HandleUsernameCheckRequestPacket);
        
        PacketManager.RegisterPacket<FetchGroupMessagesPacket>(0x14);
        PacketManager.RegisterHandler<FetchGroupMessagesPacket>(0x14, HandleFetchGroupMessagesPacket);
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
        packetToSend.Avatar = _server.Clients[_uuid].Avatar;
        packetToSend.Uuid = _uuid;
        packetToSend.GroupId = packet.GroupId;
        packetToSend.Message = packet.Message;
        
        var chatMessage = new ChatMessage(
            Guid.NewGuid(),
            packet.GroupId,
            _uuid,
            packet.Message,
            DateTime.Now
        );
        
        _server.GetMessageCache().Add(chatMessage.Uuid, chatMessage);

        _server.SendPacketToAllClients(packetToSend);

        Console.WriteLine($"[{_username}]: {packet.Message}");
    }

    public void HandleFetchGroupMessagesPacket(FetchGroupMessagesPacket packet, EventArgs args)
    {
        var messages = _server.GetLastMessagesFromGroup(packet.GroupId);
        foreach (var message in messages)
        {
            var client = _server.GetClientByUuid(message.SenderUuid);
            if (client == null)
            {
                continue;
            }
            
            var messagePacket = new ServerSidePackets.TextChatMessageHistoryPacket();
            messagePacket.Username = client.Username;
            messagePacket.Avatar = client.Avatar;
            messagePacket.Uuid = message.Uuid;
            messagePacket.GroupId = message.GroupUuid;
            messagePacket.Message = message.Content;
            
            Writer.WritePacket(messagePacket);
        }
        
        Writer.Flush(Context);
    }

    public void HandleFetchContactsPacket(FetchContactsPacket packet, EventArgs args)
    {
        Console.WriteLine($"Fetching contacts for {_username}");
        
        var clientStatusPacket = new ServerSidePackets.ClientStatusPacket();
        clientStatusPacket.Avatar = _server.Clients[_uuid].Avatar;
        clientStatusPacket.Status = _server.Clients[_uuid].Status;
        
        Writer.WritePacket(clientStatusPacket);
        Writer.Flush(Context);
        
        var groups = _server.GetGroupsByClientId(_uuid);
        
        foreach (var group in groups)
        {
            if (group.privateGroup)
            {
                var otherUser = _server.GetOtherClientInPrivateGroup(group.GroupId, _uuid);
                if (otherUser == null)
                {
                    continue;
                }
                
                var contactsPacketPrivate = new ServerSidePackets.ClientContactPacket();
                contactsPacketPrivate.Uuid = group.GroupId;
                contactsPacketPrivate.Username = otherUser.Username;
                contactsPacketPrivate.Avatar = otherUser.Avatar;
                Writer.WritePacket(contactsPacketPrivate);
                continue;
            }
            
            var contactsPacket = new ServerSidePackets.ClientContactPacket();
            contactsPacket.Uuid = group.GroupId;
            contactsPacket.Username = group.Name;
            contactsPacket.Avatar = group.Avatar;
            Writer.WritePacket(contactsPacket);
        }
        
        Writer.Flush(Context);
    }
    
    public void HandleLoginRequestPacket(LoginRequestPacket packet, EventArgs args)
    {
        var user = _server.GetClientByUsername(packet.Login.ToLower());
        if (user == null)
        {
            var userNotExistsPacket = new ServerSidePackets.LoginFailedPacket();
            userNotExistsPacket.Reason = "User does not exists!";
            Writer.WritePacket(userNotExistsPacket);
            Writer.Flush(Context);
            return;
        }
        
        if (!BCrypt.Net.BCrypt.Verify(packet.Password, user.Password))
        {
            var loginFailedBadPassword = new ServerSidePackets.LoginFailedPacket();
            loginFailedBadPassword.Reason = "Bad login or password!";
            Writer.WritePacket(loginFailedBadPassword);
            Writer.Flush(Context);
            return;
        }
        
        user.Handler = this;
        _uuid = user.Uuid;
        _username = user.Username;
        
        _server.Clients.Add(_uuid, user);
        
        var loginSuccess = new ServerSidePackets.LoginSuccess();
        loginSuccess.Username = _username;
        loginSuccess.Uuid = _uuid;
        
        Writer.WritePacket(loginSuccess);
        Writer.Flush(Context);
    }
    
    public void HandleRegisterRequestPacket(RegisterRequestPacket packet, EventArgs args)
    {
        var user = _server.GetClientByUsername(packet.Login.ToLower());
        if (user != null)
        {
            return;
        }

        if (packet.Login.Length > 25)
        {
            return;
        }

        var client = new Client(Guid.NewGuid(), packet.Login.ToLower(), packet.Password, null);
        client.EncryptPassword();
        
        _server.GetClientCache().Add(client.Uuid, client);
    }
    
    public void HandleUsernameCheckRequestPacket(UsernameCheckRequestPacket packet, EventArgs args)
    {
        var user = _server.GetClientByUsername(packet.Username.ToLower());
        if (user == null)
        {
            var usernameCheckResponse = new ServerSidePackets.UsernameCheckResponsePacket();
            usernameCheckResponse.Username = packet.Username;
            usernameCheckResponse.Exists = true;

            Writer.WritePacket(usernameCheckResponse);
            Writer.Flush(Context);
            return;
        }
        
        var usernameCheckResponse2 = new ServerSidePackets.UsernameCheckResponsePacket();
        usernameCheckResponse2.Username = packet.Username;
        usernameCheckResponse2.Exists = false;

        Writer.WritePacket(usernameCheckResponse2);
        Writer.Flush(Context);
    }
    
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