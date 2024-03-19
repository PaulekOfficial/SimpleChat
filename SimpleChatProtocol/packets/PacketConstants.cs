using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatProtocol
{
    public class PacketConstants
    {
        public const int ProtocolNumber = 1;
        public const int Threshold = 8192;
        public const int MaxPacketId = 15;
        public const int MaxPacketLength = 1000000;
    }
}
