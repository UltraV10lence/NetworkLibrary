namespace NetworkLibrary;

internal interface IPacketListener {
    public Type Type { get; }
    public void Consume(object obj);
}

internal class PacketListener<T> : IPacketListener {
    public Type Type { get; }
    public readonly Action<T> Consumer;
    
    public PacketListener(Action<T> consumer) {
        Type = typeof(T);
        Consumer = consumer;
    }

    public void Consume(object obj) {
        if (obj.GetType() != Type)
            throw new ArgumentException($"Cannot consume packet of type other than registered type. Expected {Type}, got {obj.GetType()}");
        Consumer.Invoke((T) obj);
    }
}