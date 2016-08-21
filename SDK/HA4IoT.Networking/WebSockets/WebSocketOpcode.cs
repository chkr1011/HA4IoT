namespace HA4IoT.Networking.WebSockets
{
    public enum WebSocketOpcode : byte
    {
        // Documentation is in LSB. Values here are MSB instead
        Continuation = 0, // 0x0
        Text = 128, // 0x1
        Binary = 64, // 0x2
        // 0x3-0x7 have no meaning,
        ConnectionClose = 16, // 0x8,
        Ping = 144, // 0x9,
        Pong = 80 // 0xA
    }
}
