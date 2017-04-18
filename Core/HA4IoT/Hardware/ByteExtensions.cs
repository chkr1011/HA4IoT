using System;

namespace HA4IoT.Hardware
{
    public static class ByteExtensions
    {
        public static bool GetBit(this byte[] bytes, int index)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (index > bytes.Length * 8 - 1) throw new ArgumentOutOfRangeException(nameof(index));

            var byteOffset = index / 8;
            var bitOffset = index % 8;

            return GetBit(bytes[byteOffset], bitOffset);
        }

        public static void SetBit(this byte[] bytes, int index, bool state)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (index > bytes.Length * 8 - 1) throw new ArgumentOutOfRangeException(nameof(index));

            var byteOffset = index / 8;
            var bitOffset = index % 8;

            bytes[byteOffset] = (byte)SetBit(bytes[byteOffset], bitOffset, state);
        }

        private static bool GetBit(this ulong source, int index)
        {
            if (index > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (source & (0x1UL << index)) > 0UL;
        }

        private static ulong SetBit(this ulong target, int index, bool state)
        {
            if (index > 7) throw new ArgumentOutOfRangeException(nameof(index));

            if (state)
            {
                // Byte: 01010101
                // Mask: 00000010 (00000001 is moved left)
                // Combined with "|" (or) this will result in:
                //       01010111
                return (byte)((0x1UL << index) | target);
            }

            // Byte: 01010101
            // Mask: 11111011 (00000001 is moved left and then negated using "~")
            // Combined with "&" (and) this will result in:
            //       01010001
            return (byte)(~(0x1UL << index) & target);
        }
    }
}
