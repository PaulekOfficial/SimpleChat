using System.Text;
using DotNetty.Buffers;

namespace SimpleChatProtocol;

public class ServerSidePackets
{
    public class EncryptionRequest : IPacket
    {
        private Guid _serverID;

        public Guid ServerId
        {
            get => _serverID;
            set => _serverID = value;
        }

        public byte[] PublicKey { get; set; }

        public string VerifyToken { get; set; }

        public string EncryptionProtocol { get; set; }

        public byte PacketId()
        {
            return 0x0B;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var serverIdLength = byteBuffer.ReadInt();
            var guidByteBuffer = byteBuffer.ReadBytes(serverIdLength);
            _serverID = new Guid(guidByteBuffer.Array.Take(16).ToArray());

            var publicKeyLength = byteBuffer.ReadInt();
            PublicKey = new byte[publicKeyLength];
            for (var i = 0; i < publicKeyLength; i++) PublicKey[i] = byteBuffer.ReadByte();

            var verifyTokenLength = byteBuffer.ReadInt();
            VerifyToken = byteBuffer.ReadString(verifyTokenLength, Encoding.Default);

            var encryptionProtocolLength = byteBuffer.ReadInt();
            EncryptionProtocol = byteBuffer.ReadString(encryptionProtocolLength, Encoding.Default);
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(_serverID.ToByteArray().Length);
            byteBuffer.WriteBytes(_serverID.ToByteArray());

            byteBuffer.WriteInt(PublicKey.Length);
            byteBuffer.WriteBytes(PublicKey);

            byteBuffer.WriteInt(VerifyToken.Length);
            byteBuffer.WriteString(VerifyToken, Encoding.Default);

            byteBuffer.WriteInt(EncryptionProtocol.Length);
            byteBuffer.WriteString(EncryptionProtocol, Encoding.Default);
        }

        public void Dispose()
        {
            _serverID = Guid.Empty;
            PublicKey = null;
            VerifyToken = "";
            EncryptionProtocol = "";

            GC.SuppressFinalize(this);
        }
    }

    public class LoginSuccess : IPacket
    {
        public string Username { get; set; }
        public Guid Uuid { get; set; }

        public byte PacketId()
        {
            return 0x0D;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var nickLength = byteBuffer.ReadInt();
            Username = byteBuffer.ReadString(nickLength, Encoding.Default);

            var uuidLength = byteBuffer.ReadInt();
            Uuid = Guid.Parse(byteBuffer.ReadString(uuidLength, Encoding.Default));
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(Username.Length);
            byteBuffer.WriteString(Username, Encoding.Default);

            byteBuffer.WriteInt(Uuid.ToString().Length);
            byteBuffer.WriteString(Uuid.ToString(), Encoding.Default);
        }

        public void Dispose()
        {
            Username = "";
            Uuid = Guid.Empty;

            GC.SuppressFinalize(this);
        }
    }

    public class LoginDisconnect : IPacket
    {
        public string Reason { get; set; }

        public byte PacketId()
        {
            return 0x0E;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var reasonLength = byteBuffer.ReadInt();
            Reason = byteBuffer.ReadString(reasonLength, Encoding.Default);
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(Reason.Length);
            byteBuffer.WriteString(Reason, Encoding.Default);
        }

        public void Dispose()
        {
            Reason = "";

            GC.SuppressFinalize(this);
        }
    }

    public class TextChatMessageHistoryPacket : IPacket
    {
        public Guid Uuid { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Avatar { get; set; }
        public DateTime Time { get; set; }

        public byte PacketId()
        {
            return 0x0F;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var nicknameLength = byteBuffer.ReadInt();
            Username = byteBuffer.ReadString(nicknameLength, Encoding.Default);

            var messageLength = byteBuffer.ReadInt();
            Message = byteBuffer.ReadString(messageLength, Encoding.Default);

            var uuidLength = byteBuffer.ReadInt();
            Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));

            var avatarLength = byteBuffer.ReadInt();
            Avatar = byteBuffer.ReadString(avatarLength, Encoding.Default);

            Time = new DateTime(byteBuffer.ReadLong());
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(Username.Length);
            byteBuffer.WriteString(Username, Encoding.Default);

            byteBuffer.WriteInt(Message.Length);
            byteBuffer.WriteString(Message, Encoding.Default);

            var uuid = Uuid.ToString();
            byteBuffer.WriteInt(uuid.Length);
            byteBuffer.WriteString(uuid, Encoding.Default);

            byteBuffer.WriteInt(Avatar.Length);
            byteBuffer.WriteString(Avatar, Encoding.Default);

            byteBuffer.WriteLong(Time.Ticks);
        }

        public void Dispose()
        {
            Username = "";
            Message = "";
            Uuid = Guid.Empty;

            GC.SuppressFinalize(this);
        }
    }

    public class ClientContactPacket : IPacket
    {
        public Guid Uuid { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }


        public byte PacketId()
        {
            return 0x1A;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var uuidLength = byteBuffer.ReadInt();
            Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));

            var usernameLength = byteBuffer.ReadInt();
            Username = byteBuffer.ReadString(usernameLength, Encoding.Default);

            var avatarLength = byteBuffer.ReadInt();
            Avatar = byteBuffer.ReadString(avatarLength, Encoding.Default);
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            var uuid = Uuid.ToString();
            byteBuffer.WriteInt(uuid.Length);
            byteBuffer.WriteString(uuid, Encoding.Default);

            byteBuffer.WriteInt(Username.Length);
            byteBuffer.WriteString(Username, Encoding.Default);

            byteBuffer.WriteInt(Avatar.Length);
            byteBuffer.WriteString(Avatar, Encoding.Default);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
    
    public class UsernameCheckResponsePacket : IPacket
    {
        public string Username { get; set; }
        public Boolean Exists { get; set; }


        public byte PacketId()
        {
            return 0x1B;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var usernameLength = byteBuffer.ReadInt();
            Username = byteBuffer.ReadString(usernameLength, Encoding.Default);

            Exists = byteBuffer.ReadBoolean();
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(Username.Length);
            byteBuffer.WriteString(Username, Encoding.Default);

            byteBuffer.WriteBoolean(Exists);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
    
    public class LoginFailedPacket : IPacket
    {
        public string Reason { get; set; }

        public byte PacketId()
        {
            return 0x1C;
        }

        public PacketDirection Direction()
        {
            return PacketDirection.ServerBound;
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            var reasonLength = byteBuffer.ReadInt();
            Reason = byteBuffer.ReadString(reasonLength, Encoding.Default);
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            byteBuffer.WriteInt(Reason.Length);
            byteBuffer.WriteString(Reason, Encoding.Default);
        }

        public void Dispose()
        {
            Reason = "";

            GC.SuppressFinalize(this);
        }
    }
}