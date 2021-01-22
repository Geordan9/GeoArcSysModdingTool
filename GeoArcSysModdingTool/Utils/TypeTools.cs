using System;

namespace GeoArcSysModdingTool.Utils
{
    public static class TypeTools
    {
        public static long ByteArrayToLong(byte[] bytes, bool Is64Bit = false)
        {
            Array.Resize(ref bytes, 8);
            long value = BitConverter.ToInt32(bytes, 0);
            if (Is64Bit)
            {
                long valueext = BitConverter.ToInt32(bytes, 4);
                value = (valueext << 32) | (value & 0xFFFFFFFFL);
            }

            return value;
        }

        public static ulong ByteArrayToULong(byte[] bytes, bool Is64Bit = false)
        {
            Array.Resize(ref bytes, 8);
            ulong value = BitConverter.ToUInt32(bytes, 0);
            if (Is64Bit)
            {
                ulong valueext = BitConverter.ToUInt32(bytes, 4);
                value = (valueext << 32) | (value & 0xFFFFFFFFL);
            }

            return value;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static long HexLiteralToLong(string hex)
        {
            if (string.IsNullOrEmpty(hex)) throw new ArgumentException("hex");

            var i = hex.Length > 1 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X') ? 2 : 0;
            long value = 0;

            while (i < hex.Length)
            {
                uint x = hex[i++];

                if (x >= '0' && x <= '9') x = x - '0';
                else if (x >= 'A' && x <= 'F') x = x - 'A' + 10;
                else if (x >= 'a' && x <= 'f') x = x - 'a' + 10;
                else throw new ArgumentOutOfRangeException("hex");

                value = 16 * value + x;
            }

            return value;
        }

        public static bool IsDecimalFormat(string input)
        {
            decimal dummy;
            return decimal.TryParse(input, out dummy);
        }

        public static bool IsUnsignedIntFormat(string input)
        {
            uint dummy;
            return uint.TryParse(input, out dummy);
        }
    }
}