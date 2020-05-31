namespace Contoso.Extensions.FileSystem
{
    internal static class ByteConverterExtensions
    {
        public static void SetUInt16(this byte[] n, ulong pos, ushort value)
        {
            n[pos + 0] = (byte)value;
            n[pos + 1] = (byte)(value >> 8);
        }

        public static void SetUInt32(this byte[] n, ulong pos, uint value)
        {
            n[pos + 0] = (byte)value;
            n[pos + 1] = (byte)(value >> 8);
            n[pos + 2] = (byte)(value >> 16);
            n[pos + 3] = (byte)(value >> 24);
        }

        public static void SetUInt64(this byte[] n, ulong pos, ulong value)
        {
            n[pos + 0] = (byte)value;
            n[pos + 1] = (byte)(value >> 8);
            n[pos + 2] = (byte)(value >> 16);
            n[pos + 3] = (byte)(value >> 24);
            n[pos + 4] = (byte)(value >> 32);
            n[pos + 5] = (byte)(value >> 40);
            n[pos + 6] = (byte)(value >> 48);
            n[pos + 7] = (byte)(value >> 56);
        }
    }
}
