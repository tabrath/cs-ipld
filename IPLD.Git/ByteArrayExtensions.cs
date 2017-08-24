using System.Text;
using Multiformats.Base;

namespace IPLD.Git
{
    internal static class ByteArrayExtensions
    {
        public static string ToHex(this byte[] bytes) => Multibase.EncodeRaw(Multibase.Base16, bytes);

        public static int IndexOf(this byte[] array, byte needle)
        {
            for (var i = 0; i < array.Length; i++)
                if (array[i] == needle)
                    return i;

            return -1;
        }

        public static string AsString(this byte[] array) => Encoding.UTF8.GetString(array);
    }
}