namespace HA4IoT.Networking.Http
{
    public enum WebSocketOpcode : byte
    {
        Continuation = 0x0,
        Text = 0x1,
        Binary = 0x2,
        // 0x3-0x7 have no meaning,
        ConnectionClose = 0x8,
        Ping = 0x9,
        Pong = 0xA
    }
}
