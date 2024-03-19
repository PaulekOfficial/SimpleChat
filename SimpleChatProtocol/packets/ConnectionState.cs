using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatProtocol
{
    public enum ConnectionState
    {
        HANDSHAKE = 1,
        STATUS,
        PLAY,
        CONSOLE,
        UNKNOWN
    }
}
