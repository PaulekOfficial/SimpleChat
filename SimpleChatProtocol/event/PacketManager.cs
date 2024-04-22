namespace SimpleChatProtocol;

public class PacketManager
{
    public delegate void PacketHandler<T>(T packet, EventArgs eventArgs) where T : IPacket;

    private readonly Dictionary<byte, Delegate> _handlers = new();
    private readonly Dictionary<byte, Type> _packets = new();

    private readonly PacketReader _reader;
    
    private readonly object _lock = new object();

    public PacketManager()
    {
        _reader = new PacketReader();
    }

    public async void CallAndForget(RawPacket packet, EventArgs eventArgs)
    {
        if (!_packets.ContainsKey(packet.PacketId())) return;

        var normalPacket =
            _reader.ParsePacket(packet, Activator.CreateInstance(_packets[packet.PacketId()]) as IPacket);
        if (normalPacket == null) return;

        CallAndForget(normalPacket, eventArgs);
    }

    public async void CallAndForget(IPacket packet, EventArgs eventArgs)
    {
        if (!_handlers.ContainsKey(packet.PacketId())) return;

        await Task.Factory.StartNew(() =>
        {
            lock (_lock)
            {
                foreach (var dic in _handlers)
                {
                    if (dic.Key != packet.PacketId()) continue;

                    dic.Value.DynamicInvoke(packet, eventArgs);
                }
            }
        });
    }

    public void RegisterHandler<T>(byte packetId, PacketHandler<T> handler) where T : IPacket
    {
        Task.Factory.StartNew(() =>
        {
            lock (_lock)
            {
                if (_handlers.ContainsKey(packetId))
                {
                    _handlers[packetId] = handler;
                    return;
                }

                _handlers.Add(packetId, handler);
            }
        });
    }

    public void UnregisterHandler(byte packetId)
    {
        Task.Factory.StartNew(() =>
        {
            lock (_lock)
            {
                _handlers.Remove(packetId);
            }
        });
    }

    public void RegisterPacket<T>(byte packetId) where T : IPacket
    {
        _packets.Add(packetId, typeof(T));
    }

    public void UnregisterPacket(byte packetId)
    {
        _packets.Remove(packetId);
    }
}