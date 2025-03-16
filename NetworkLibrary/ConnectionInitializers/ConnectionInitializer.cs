using NetworkLibrary.Packets;

namespace NetworkLibrary.ConnectionInitializers;

public interface ConnectionInitializer {
    public ConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier);
    //public ConnectionInitializer SetEncryption(asd encryptor, asd decryptor);
}