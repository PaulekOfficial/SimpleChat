using DotNetty.Buffers;

namespace SimpleChatProtocol;

public interface IPacket : IDisposable
{
    byte PacketId();

    PacketDirection Direction();

    void Parse(IByteBuffer byteBuffer);

    void Serialize(IByteBuffer byteBuffer);
}