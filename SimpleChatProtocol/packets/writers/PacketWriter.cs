using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace SimpleChatProtocol
{
    public class PacketWriter : IWriter
    {
        private readonly PooledByteBufferAllocator _byteAllocator;
        private IByteBuffer _byteBuffer;

        public PacketWriter()
        {
            _byteAllocator = new PooledByteBufferAllocator();
            _byteBuffer = _byteAllocator.Buffer();
        }

        public void WritePacket(IPacket packet)
        {
            lock (_byteBuffer)
            {
                if (packet is RawPacket)
                {
                    var raw = (RawPacket)packet;
                    _byteBuffer.WriteByte(raw.PacketId());
                    _byteBuffer.WriteInt(Convert.ToInt32(raw.DataLenght));
                    _byteBuffer.WriteBytes(raw.Payload());
                }

                _byteBuffer.WriteByte(packet.PacketId());

                var payloadBuffer = _byteAllocator.Buffer();
                packet.Serialize(payloadBuffer);

                _byteBuffer.WriteInt(payloadBuffer.ReadableBytes);
                _byteBuffer.WriteBytes(payloadBuffer);
            }
        }

        public RawPacket PacketToRaw(IPacket packet)
        {
            var raw = new RawPacket();
            raw.SetID(packet.PacketId());

            var payloadBuffer = _byteAllocator.Buffer();
            packet.Serialize(payloadBuffer);
            raw.SetPayload(payloadBuffer.Copy());
            raw.SetDataLenght(payloadBuffer.ReadableBytes);

            return raw;
        }

        public int GetStoredBytesLength()
        {
            return _byteBuffer.ReadableBytes;
        }

        public void Flush(IChannelHandlerContext context)
        {
            lock (_byteBuffer)
            {
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(_byteBuffer));
                _byteBuffer = _byteAllocator.Buffer(PacketConstants.Threshold);
            }
        }
    }
}
