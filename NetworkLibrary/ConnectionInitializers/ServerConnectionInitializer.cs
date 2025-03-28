using NetworkLibrary.Packets;

namespace NetworkLibrary.ConnectionInitializers;

public interface ServerConnectionInitializer : ConnectionInitializer {
    public string RemoteIp { get; }
    public ushort RemotePort { get; }
    public new ServerConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier);
}