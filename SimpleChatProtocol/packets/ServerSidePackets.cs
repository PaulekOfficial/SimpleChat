using DotNetty.Buffers;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatProtocol
{
    public class ServerSidePackets
    {
        #region Direction ClientBound

        #endregion

        #region Direction ServerBound

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
            public string Username { get; set; }
            public string Message { get; set; }
            public Guid   Uuid { get; set; }

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
            }

            public void Dispose()
            {
                Username = "";
                Message = "";

                GC.SuppressFinalize(this);
            }
        }

        public class UserJoinChatPacket : IPacket
        {
            public string Username { get; set; }
            public Guid Uuid { get; set; }

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

                var uuidLength = byteBuffer.ReadInt();
                Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));
            }

            public void Serialize(IByteBuffer byteBuffer)
            {
                byteBuffer.WriteInt(Username.Length);
                byteBuffer.WriteString(Username, Encoding.Default);

                var uuid = Uuid.ToString();
                byteBuffer.WriteInt(uuid.Length);
                byteBuffer.WriteString(uuid, Encoding.Default);
            }

            public void Dispose()
            {
                Username = "";

                GC.SuppressFinalize(this);
            }
        }

        public class UserLeaveChatPacket : IPacket
        {
            public string Username { get; set; }
            public Guid Uuid { get; set; }

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

                var uuidLength = byteBuffer.ReadInt();
                Uuid = new Guid(byteBuffer.ReadString(uuidLength, Encoding.Default));
            }

            public void Serialize(IByteBuffer byteBuffer)
            {
                byteBuffer.WriteInt(Username.Length);
                byteBuffer.WriteString(Username, Encoding.Default);

                var uuid = Uuid.ToString();
                byteBuffer.WriteInt(uuid.Length);
                byteBuffer.WriteString(uuid, Encoding.Default);
            }

            public void Dispose()
            {
                Username = "";

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
