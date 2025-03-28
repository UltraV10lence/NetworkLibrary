using System.Text;
using NetworkLibrary.Packets.SystemPackets;

namespace NetworkLibrary.Packets;

public class PacketTypeIdentifier {
    public delegate void Encoder<in T>(ByteBuf buf, T obj);
    public delegate T Decoder<out T>(ByteBuf buf);

    private readonly Dictionary<short, IRegisteredPacket> registeredTypes = new();

    public PacketTypeIdentifier() {
        InitializeSystemPacketTypes();
    }

    private void InitializeSystemPacketTypes() {
        RegisterPacket<PingPacket>(-2, (_, _) => { }, _ => new PingPacket(), true);
    }

    public void RegisterPacket<T>(short typeId, Encoder<T> encoder, Decoder<T> decoder) {
        RegisterPacket(typeId, encoder, decoder, false);
    }
    
    internal void RegisterPacket<T>(short typeId, Encoder<T> encoder, Decoder<T> decoder, bool systemPacket) {
        if (typeId < 0 && !systemPacket)
            throw new ArgumentException("Cannot register system id for packet.");
        registeredTypes.Add(typeId, new RegisteredPacket<T>(encoder, decoder));
    }

    public object Decode(short id, ByteBuf buf) {
        if (!registeredTypes.TryGetValue(id, out var type))
            throw new ArgumentException($"Packet ID {id} is not registered.", nameof(id));
        
        return type.Decode(buf);
    }

    public short FetchPacketId(object obj) {
        var type = obj.GetType();
        var registeredType = registeredTypes.SingleOrDefault(rt => rt.Value.Type == type);
        if (registeredType.Equals(default(KeyValuePair<short, IRegisteredPacket>)))
            throw new ArgumentException($"No registered type matches {type}", nameof(obj));

        return registeredType.Key;
    }

    public void Encode(Stream to, object obj, short fetchedId, TcpConnection connection) {
        var registeredType = registeredTypes[fetchedId];
        
        using (var buffer = new ByteBuf()) {
            registeredType.Encode(buffer, obj);
            buffer.EnterReadOnlyMode();

            try {
                using (var writer = new BinaryWriter(to, Encoding.UTF8, true)) {
                    writer.Write((int) buffer.Length);
                    writer.Write(fetchedId);
                }
                buffer.CopyTo(to);
            } catch {
                connection.Disconnect(DisconnectReason.StreamClosed);
            }
        }
    }

    private interface IRegisteredPacket {
        Type Type { get; }
        object Decode(ByteBuf buf);
        void Encode(ByteBuf buf, object obj);
    }

    private class RegisteredPacket<T> : IRegisteredPacket {
        private readonly Encoder<T> encoder;
        private readonly Decoder<T> decoder;

        public Type Type => typeof(T);

        public RegisteredPacket(Encoder<T> encoder, Decoder<T> decoder) {
            this.encoder = encoder;
            this.decoder = decoder;
        }

        public object Decode(ByteBuf buf) => decoder(buf);

        public void Encode(ByteBuf buf, object obj) {
            if (obj is not T castedObj)
                throw new ArgumentException($"Object is not of type {typeof(T)}", nameof(obj));
            encoder(buf, castedObj);
        }
    }
}