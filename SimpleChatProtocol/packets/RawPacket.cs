using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatProtocol
{
    public class RawPacket : IPacket
    {
        private byte _id;
        private IByteBuffer _payload;

        public RawPacket()
        {
        }

        public RawPacket(byte id, int dataLenght, IByteBuffer payload)
        {
            _id = id;
            DataLenght = dataLenght;
            _payload = payload;
        }

        public int DataLenght { get; set; }

        public byte PacketId()
        {
            return _id;
        }

        public PacketDirection Direction()
        {
            throw new Exception("Raw packet is a container, the direction is not known");
        }

        public void Parse(IByteBuffer byteBuffer)
        {
            throw new Exception("Raw packet is a container, cannot parse packet from it");
        }

        public void Serialize(IByteBuffer byteBuffer)
        {
            throw new Exception("Raw packet is a container, cannot serialize packet from it");
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SetID(byte id)
        {
            _id = id;
        }

        public void SetDataLenght(int lenght)
        {
            DataLenght = lenght;
        }

        public void SetPayload(IByteBuffer payload)
        {
            _payload = payload;
        }

        public IByteBuffer Payload()
        {
            return _payload;
        }
    }
}
