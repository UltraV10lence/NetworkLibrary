namespace NetworkLibrary;

public enum DisconnectReason {
    Disconnect,
    Reconnect,
    Exception,
    StreamClosed,
    Closing,
    Timeout
}