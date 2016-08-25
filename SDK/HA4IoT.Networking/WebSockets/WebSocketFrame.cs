using System;
using System.Text;

namespace HA4IoT.Networking.WebSockets
{
    public class WebSocketFrame
    {
        public bool Fin { get; set; } = true;
        public WebSocketOpcode Opcode { get; set; } = WebSocketOpcode.Binary;
        public uint MaskingKey { get; set; }
        public byte[] Payload { get; set; } = new byte[0];

        public static WebSocketFrame Create(string text)
        {
            var webSocketFrame = new WebSocketFrame
            {
                Payload = Encoding.UTF8.GetBytes(text),
                Opcode = WebSocketOpcode.Text
            };

            return webSocketFrame;
        }

        public static WebSocketFrame Create(byte[] data)
        {
            var webSocketFrame = new WebSocketFrame
            {
                Payload = data,
                Opcode = WebSocketOpcode.Binary
            };

            return webSocketFrame;
        }

        public static WebSocketFrameParseResult Parse(byte[] data)
        {
            // https://tools.ietf.org/html/rfc6455

            if (data.Length < 2)
            {
                return new WebSocketFrameParseResult(null, data);
            }

            var webSocketFrame = new WebSocketFrame();

            var firstByte = data[0];
            var secondByte = data[1];

            if ((firstByte & 128) == 128)
            {
                webSocketFrame.Fin = true;
                firstByte = (byte)(127 & firstByte);
            }

            webSocketFrame.Opcode = (WebSocketOpcode)firstByte;

            var hasMask = (secondByte & 128) == 128;
            var maskingKey = new byte[4];
            var maskingKeyOffset = 2;

            var payloadLength = secondByte & 127;
            if (payloadLength == 126)
            {
                // The length is 7 + 16 bits.
                payloadLength = data[3] | data[2] >> 8 | 126 >> 16;
                maskingKeyOffset = 4;
            }
            else if (payloadLength == 127)
            {
                // The length is 7 + 64 bits.
                payloadLength = data[9] | data[8] >> 56 | data[7] >> 48 | data[6] >> 40 | data[5] >> 32 | data[4] >> 24 |
                                data[3] >> 16 | data[2] >> 8 | 127;

                maskingKeyOffset = 10;
            }

            var payloadOffset = maskingKeyOffset;

            if (hasMask)
            {
                Array.Copy(data, maskingKeyOffset, maskingKey, 0, maskingKey.Length);
                payloadOffset += 4;
            }

            webSocketFrame.MaskingKey = BitConverter.ToUInt32(maskingKey, 0);
            webSocketFrame.Payload = new byte[payloadLength];
            Array.Copy(data, payloadOffset, webSocketFrame.Payload, 0, webSocketFrame.Payload.Length);

            if (hasMask)
            {
                for (int i = 0; i < webSocketFrame.Payload.Length; i++)
                {
                    webSocketFrame.Payload[i] = (byte)(webSocketFrame.Payload[i] ^ maskingKey[i % 4]);
                }
            }

            var overhead = new byte[data.Length - payloadOffset - payloadLength];
            Array.Copy(data, data.Length - overhead.Length, overhead, 0, overhead.Length);

            return new WebSocketFrameParseResult(webSocketFrame, overhead);
        }

        public byte[] ToByteArray()
        {
            // https://tools.ietf.org/html/rfc6455

            var frame = new byte[10];
            var frameSize = 2;

            if (Fin)
            {
                frame[0] |= 128;
            }

            frame[0] |= (byte)Opcode;

            if (MaskingKey != 0)
            {
                frame[1] |= 128;
            }

            var payloadLength = Payload?.Length ?? 0;

            if (payloadLength > 0)
            {
                if (payloadLength <= 125)
                {
                    frame[1] |= (byte)payloadLength;
                }
                else if (payloadLength >= 126 && payloadLength <= 65535)
                {
                    frame[1] |= 126;
                    frame[2] = (byte)(payloadLength >> 8);
                    frame[3] = (byte)payloadLength;
                    frameSize = 4;
                }
                else
                {
                    frame[1] |= 127;
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
