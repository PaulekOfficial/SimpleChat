using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatProtocol
{
    public interface IPacket : IDisposable
    {
        byte PacketId();

        PacketDirection Direction();

        void Parse(IByteBuffer byteBuffer);

        void Serialize(IByteBuffer byteBuffer);
    }
}
