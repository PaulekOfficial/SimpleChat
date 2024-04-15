using DotNetty.Transport.Channels;

namespace SimpleChatProtocol;

public interface IWriter
{
    void WritePacket(IPacket packet);

    RawPacket PacketToRaw(IPacket packet);

    int GetStoredBytesLength();

    void Flush(IChannelHandlerContext context);
}