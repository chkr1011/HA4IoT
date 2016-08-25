namespace HA4IoT.Networking.WebSockets
{
    public enum WebSocketOpcode : byte
    {
        // Values here including the FIN bit set to 0.
        Continuation = 0, // 0x0
        Text = 128, // 0x1
        Binary = 64, // 0x2
        // 0x3-0x7 have no meaning,
        ConnectionClose = 16, // 0x8,
        Ping = 144, // 0x9,
        Pong = 80 // 0xA
    }
}
