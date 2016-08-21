using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Networking.Http
{
    public class WebSocketFrame
    {
        public bool Fin{ get; set; }
        public bool Mask { get; set; }
        public WebSocketOpcode Opcode { get; set; }
        public byte[] MaskingKey { get; set; }
        public byte[] Payload { get; set; }

        public static WebSocketFrame Create(byte[] payload)
        {
            var webSocketFrame = new WebSocketFrame
            {
                Fin = true,
                Opcode = WebSocketOpcode.Text,
                Payload = payload
            };

            return webSocketFrame;
        }

        public static WebSocketFrame FromByteArray(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray()
        {
            byte[] frame = new byte[10];
            int frameSize;

            frame[0] = 129; // FIN and TEXT

            if (Payload.Length <= 125)
            {
                frame[1] = (byte)Payload.Length;
                frameSize = 2;
            }
            else if (Payload.Length >= 126 && Payload.Length <= 65535)
            {
                frame[1] = 126;
                frame[2] = (byte)((Payload.Length >> 8) & 255);
                frame[3] = (byte)(Payload.Length & 255);
                frameSize = 4;
            }
            else
            {
                frame[1] = 127;
                frame[2] = (byte)((Payload.Length >> 56) & 255);
                frame[3] = (byte)((Payload.Length >> 48) & 255);
                frame[4] = (byte)((Payload.Length >> 40) & 255);
                frame[5] = (byte)((Payload.Length >> 32) & 255);
                frame[6] = (byte)((Payload.Length >> 24) & 255);
                frame[7] = (byte)((Payload.Length >> 16) & 255);
                frame[8] = (byte)((Payload.Length >> 8) & 255);
                frame[9] = (byte)(Payload.Length & 255);
                frameSize = 10;
            }

            var buffer = new byte[frameSize + Payload.Length];
            Array.Copy(frame, 0, buffer, 0, frameSize);
            Array.Copy(Payload, 0, buffer, frameSize, Payload.Length);

            return buffer;
        }
    }
}
