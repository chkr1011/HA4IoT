using System;

namespace HA4IoT.Hardware
{
    public static class ByteExtensions
    {
        public static bool GetBit(this byte @byte, int index)
        {
            if (index > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (@byte & (0x1 << index)) > 0;
        }

        public static bool GetBit(this byte[] bytes, int index)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (index > (bytes.Length * 8) - 1) throw new ArgumentOutOfRangeException(nameof(index));

            int byteOffset = index / 8;
            int bitOffset = index % 8;

            return GetBit(bytes[byteOffset], bitOffset);
        }

        public static byte SetBit(this byte @byte, int index, bool state)
        {
            if (index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (state)
            {
                // Byte: 01010101
                // Mask: 00000010 (00000001 is moved left)
                // Combined with "|" (or) this will result in:
                //       01010111
                return (byte)((0x1 << index) | @byte);
            }

            // Byte: 01010101
            // Mask: 11111011 (00000001 is moved left and then negated using "~")
            // Combined with "&" (and) this will result in:
            //       01010001
            return (byte)(~(0x1 << index) & @byte);
        }

        public static void SetBit(this byte[] bytes, int index, bool state)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (index > (bytes.Length * 8) - 1) throw new ArgumentOutOfRangeException(nameof(index));

            int byteOffset = index / 8;
            int bitOffset = index % 8;

            bytes[byteOffset] = SetBit(bytes[byteOffset], bitOffset, state);
        }

        public static byte[] Parse(string source)
        {
            if (source == null) return null;
            if (source.Length == 0) return new byte[0];

            string[] bytes = source.Split(',');
            var buffer = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                buffer[i] = byte.Parse(bytes[i]);
            }

            return buffer;
        }

        public static string ToString(byte[] value)
        {
            if (value == null)
            {
                return null;
            }

            // The handline of short byte arrays is implemented directly to improve performance.
            switch (value.Length)
            {
                case 0:
                    {
                        return string.Empty;
                    }

                case 1:
                    {
                        return value[0].ToString();
                    }

                case 2:
                    {
                        return value[0] + "," + value[1];
                    }

                case 3:
                    {
                        return value[0] + "," + value[1] + "," + value[2];
                    }

                case 4:
                    {
                        return value[0] + "," + value[1] + "," + value[2] + "," + value[3];
                    }

                default:
                    {
                        var stringBuilder = new System.Text.StringBuilder();
                        for (int i = 0; i < value.Length; i++)
                        {
                            if (i != 0)
                            {
                                stringBuilder.Append(',');
                            }

                            stringBuilder.Append(value[i].ToString());
                        }

                        return stringBuilder.ToString();
                    }
            }
        }
    }
}
