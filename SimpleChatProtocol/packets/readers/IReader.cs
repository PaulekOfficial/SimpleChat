using DotNetty.Buffers;

namespace SimpleChatProtocol
{
    public interface IReader
    {
        RawPacket ReadPacket(IByteBuffer byteBuffer);

        IByteBuffer Payload(RawPacket packet);

        IPacket ParsePacket(List<RawPacket> rawPackets, params IPacket[] expectedPackets);

        public IPacket? ParsePacket(RawPacket rawPacket, params IPacket[] expectedPackets);
    }
}
