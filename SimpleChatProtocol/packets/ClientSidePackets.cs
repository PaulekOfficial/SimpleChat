using System.Text;
using DotNetty.Buffers;

namespace SimpleChatProtocol;

public class Handshake : IPacket
{
    public int ProtocolNumber { get; set; }

    public string ServerAddress { get; set; }

    public int Port { get; set; }

    public string Region { get; set; }

    public string System { get; set; }

    public ConnectionState NextConnectionState { get; set; }

    public byte PacketId()
    {
        return 0x00;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        ProtocolNumber = byteBuffer.ReadInt();

        var serverAddressLength = byteBuffer.ReadInt();
        ServerAddress = byteBuffer.ReadString(serverAddressLength, Encoding.Default);

        Port = byteBuffer.ReadInt();

        var regionLength = byteBuffer.ReadInt();
        Region = byteBuffer.ReadString(regionLength, Encoding.Default);

        var systemLength = byteBuffer.ReadInt();
        System = byteBuffer.ReadString(systemLength, Encoding.Default);

        var connectionStateId = byteBuffer.ReadInt();
        switch (connectionStateId)
        {
            case 1:
                NextConnectionState = ConnectionState.HANDSHAKE;
                break;
            case 2:
                NextConnectionState = ConnectionState.STATUS;
                break;
            case 3:
                NextConnectionState = ConnectionState.PLAY;
                break;
            case 4:
                NextConnectionState = ConnectionState.CONSOLE;
                break;
            default:
                NextConnectionState = ConnectionState.UNKNOWN;
                break;
        }
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(ProtocolNumber);

        byteBuffer.WriteInt(ServerAddress.Length);
        byteBuffer.WriteString(ServerAddress, Encoding.Default);

        byteBuffer.WriteInt(Port);

        byteBuffer.WriteInt(Region.Length);
        byteBuffer.WriteString(Region, Encoding.Default);

        byteBuffer.WriteInt(System.Length);
        byteBuffer.WriteString(System, Encoding.Default);

        byteBuffer.WriteInt((int)NextConnectionState);
    }

    public void Dispose()
    {
        ProtocolNumber = 0;
        ServerAddress = "";
        Port = 0;
        Region = "";
        System = "";
        NextConnectionState = ConnectionState.UNKNOWN;

        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        return
            $"Handshake: {ProtocolNumber}, {ServerAddress}, {Port}, {Region}, {System}, {NextConnectionState}";
    }
}

public class LoginStartRequest : IPacket
{
    public string UUID { get; set; }
    
    public byte PacketId()
    {
        return 0x0A;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var uuidLength = byteBuffer.ReadInt();
        UUID = byteBuffer.ReadString(uuidLength, Encoding.Default);
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(UUID.Length);
        byteBuffer.WriteString(UUID, Encoding.Default);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class EncryptionResponse : IPacket
{
    public string SharedSecret { get; set; }

    public string SharedToken { get; set; }

    public byte PacketId()
    {
        return 0x0C;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var sharedSecretLength = byteBuffer.ReadInt();
        SharedSecret = byteBuffer.ReadString(sharedSecretLength, Encoding.Default);

        var verifyTokenLength = byteBuffer.ReadInt();
        SharedToken = byteBuffer.ReadString(verifyTokenLength, Encoding.Default);
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(SharedSecret.Length);
        byteBuffer.WriteString(SharedSecret, Encoding.Default);

        byteBuffer.WriteInt(SharedToken.Length);
        byteBuffer.WriteString(SharedToken, Encoding.Default);
    }

    public void Dispose()
    {
        SharedSecret = "";
        SharedToken = "";

        GC.SuppressFinalize(this);
    }
}

public class TextChatPacket : IPacket
{
    public Guid Uuid { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }

    public byte PacketId()
    {
        return 0x0D;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var messageLength = byteBuffer.ReadInt();
        Message = byteBuffer.ReadString(messageLength, Encoding.Default);

        var uuidLength = byteBuffer.ReadInt();
        Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));

        Time = new DateTime(byteBuffer.ReadLong());
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(Message.Length);
        byteBuffer.WriteString(Message, Encoding.Default);

        var uuid = Uuid.ToString();
        byteBuffer.WriteInt(uuid.Length);
        byteBuffer.WriteString(uuid, Encoding.Default);

        byteBuffer.WriteLong(Time.Ticks);
    }

    public void Dispose()
    {
        Message = "";
        Uuid = Guid.Empty;

        GC.SuppressFinalize(this);
    }
}

public class FetchContactsPacket : IPacket
{
    public Guid Uuid { get; set; }

    public byte PacketId()
    {
        return 0x10;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var uuidLength = byteBuffer.ReadInt();
        Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        var uuid = Uuid.ToString();
        byteBuffer.WriteInt(uuid.Length);
        byteBuffer.WriteString(uuid, Encoding.Default);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class LoginRequestPacket : IPacket
{
    public String Login { get; set; }
    public String Password { get; set; }

    public byte PacketId()
    {
        return 0x11;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var loginLength = byteBuffer.ReadInt();
        Login = byteBuffer.ReadString(loginLength, Encoding.Default);
        
        var passwordLength = byteBuffer.ReadInt();
        Password = byteBuffer.ReadString(passwordLength, Encoding.Default);
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(Login.Length);
        byteBuffer.WriteString(Login, Encoding.Default);
        
        byteBuffer.WriteInt(Password.Length);
        byteBuffer.WriteString(Password, Encoding.Default);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class RegisterRequestPacket : IPacket
{
    public String Login { get; set; }
    public String Password { get; set; }

    public byte PacketId()
    {
        return 0x12;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var loginLength = byteBuffer.ReadInt();
        Login = byteBuffer.ReadString(loginLength, Encoding.Default);
        
        var passwordLength = byteBuffer.ReadInt();
        Password = byteBuffer.ReadString(passwordLength, Encoding.Default);
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(Login.Length);
        byteBuffer.WriteString(Login, Encoding.Default);
        
        byteBuffer.WriteInt(Password.Length);
        byteBuffer.WriteString(Password, Encoding.Default);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class UsernameCheckRequestPacket : IPacket
{
    public String Username { get; set; }

    public byte PacketId()
    {
        return 0x13;
    }

    public PacketDirection Direction()
    {
        return PacketDirection.ClientBound;
    }

    public void Parse(IByteBuffer byteBuffer)
    {
        var loginLength = byteBuffer.ReadInt();
        Username = byteBuffer.ReadString(loginLength, Encoding.Default);
    }

    public void Serialize(IByteBuffer byteBuffer)
    {
        byteBuffer.WriteInt(Username.Length);
        byteBuffer.WriteString(Username, Encoding.Default);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
