using DotNetty.Buffers;

namespace SimpleChatProtocol
{
    public class PacketReader : IReader
    {
        private readonly IByteBufferAllocator _byteAllocator = new PooledByteBufferAllocator();

        private IByteBuffer _storedBytes;

        public PacketReader()
        {
            _storedBytes = _byteAllocator.Buffer(0);
        }

        public RawPacket? ReadPacket(IByteBuffer byteBuffer)
        {
            var rawPacket = new RawPacket();

            var buffer = byteBuffer;

            if (_storedBytes.ReadableBytes > 0)
            {
                _storedBytes = new CompositeByteBuffer(_byteAllocator, false, 2, _storedBytes, byteBuffer);
                byteBuffer.SetReaderIndex(byteBuffer.ReadableBytes);

                buffer = _storedBytes;
            }

            var packetId = buffer.ReadByte();
            if (packetId > PacketConstants.MaxPacketId)
                throw new Exception($"RawPacket id below 256, not possible situation id: {packetId}");

            rawPacket.SetID(packetId);
            var dataLength = buffer.ReadInt();
            if (dataLength <= 0)
            {
                return null;
            }

            if (dataLength > PacketConstants.MaxPacketLength)
            {
                throw new Exception(
                    $"RawPacket payload length larger than {PacketConstants.MaxPacketLength}, not possible situation ");
                return null;
            }

            if (dataLength > buffer.ReadableBytes)
            {
                byteBuffer.SetReaderIndex(byteBuffer.ReaderIndex - 5);
                _storedBytes = new CompositeByteBuffer(_byteAllocator, false, 2, _storedBytes, byteBuffer);

                byteBuffer.SetReaderIndex(byteBuffer.ReadableBytes);
                return null;
            }

            rawPacket.SetDataLenght(dataLength);
            rawPacket.SetPayload(buffer.Copy());

            buffer.SetReaderIndex(buffer.ReaderIndex + dataLength);

            return rawPacket;
        }

        public IByteBuffer Payload(RawPacket rawPacket)
        {
            return rawPacket.Payload();
        }

        public IPacket? ParsePacket(List<RawPacket> rawPackets, params IPacket[] expectedPackets)
        {
            var rawPacket = rawPackets[0];
            if (rawPacket == null) return null;

            foreach (var packet in expectedPackets)
            {
                if (packet.PacketId() != rawPacket.PacketId()) continue;

                var payload = Payload(rawPacket);
                packet.Parse(payload);

                return packet;
            }

            throw new Exception($"Unexpected packet id: {rawPacket.PacketId()}");
        }
        
        public IPacket? ParsePacket(RawPacket rawPacket, params IPacket[] expectedPackets)
        {
            if (rawPacket == null) return null;

            foreach (var packet in expectedPackets)
            {
                if (packet.PacketId() != rawPacket.PacketId()) continue;

                var payload = Payload(rawPacket);
                packet.Parse(payload);

                return packet;
            }

            throw new Exception($"Unexpected packet id: {rawPacket.PacketId()}");
        }
    }
}
