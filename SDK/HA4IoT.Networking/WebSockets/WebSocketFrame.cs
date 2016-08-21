using System;
using HA4IoT.Hardware;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketFrame
    {
        public bool Fin { get; set; } = true;
        public WebSocketOpcode Opcode { get; set; } = WebSocketOpcode.Text;
        public byte[] MaskingKey { get; set; }
        public byte[] Payload { get; set; }

        public static WebSocketFrame Create(byte[] payload)
        {
            var webSocketFrame = new WebSocketFrame
            {
                Payload = payload
            };

            return webSocketFrame;
        }

        public static WebSocketFrame FromByteArray(byte[] data)
        {
            var webSocketFrame = new WebSocketFrame();
            if ((data[0] & 0x1) > 0)
            {
                webSocketFrame.Fin = true;
                data[0] = data[0].SetBit(0, false);
            }

            webSocketFrame.Opcode = (WebSocketOpcode)data[0];

            // TODO: Parse payload, mask etc. And Apply mask!

            return webSocketFrame;
        }

        public byte[] ToByteArray()
        {
            // RFC is written in LSB. The code here is in MSB.
            // https://tools.ietf.org/html/rfc6455

            var frame = new byte[10];
            var frameSize = 2;

            if (Fin)
            {
                frame[0] |= 1;
            }

            frame[0] |= (byte)Opcode;

            if (MaskingKey != null && MaskingKey.Length > 0)
            {
                frame[1] |= 128;
            }

            var payloadLength = Payload?.Length ?? 0;

            if (payloadLength > 0)
            {
                if (payloadLength <= 125)
                {
                    frame[1] = (byte)payloadLength;
                }
                else if (payloadLength >= 126 && payloadLength <= 65535)
                {
                    frame[1] = 126;
                    frame[2] = (byte)(payloadLength >> 8);
                    frame[3] = (byte)payloadLength;
                    frameSize = 4;
                }
                else
                {
                    frame[1] = 127;
                    frame[2] = (byte)(payloadLength >> 56);
                    frame[3] = (byte)(payloadLength >> 48);
                    frame[4] = (byte)(payloadLength >> 40);
                    frame[5] = (byte)(payloadLength >> 32);
                    frame[6] = (byte)(payloadLength >> 24);
                    frame[7] = (byte)(payloadLength >> 16);
                    frame[8] = (byte)(payloadLength >> 8);
                    frame[9] = (byte)payloadLength;
                    frameSize = 10;
                }
            }

            var buffer = new byte[frameSize + payloadLength];
            Array.Copy(frame, 0, buffer, 0, frameSize);
            Array.Copy(Payload, 0, buffer, frameSize, payloadLength);

            return buffer;
        }
    }
}
