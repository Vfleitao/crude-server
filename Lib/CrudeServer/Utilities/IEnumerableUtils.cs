using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ConnectUs.Utilities
{
    public static class IEnumerableUtils
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] arr, int size)
        {
            for (int i = 0; i < arr.Length / size + 1; i++)
            {
                yield return arr.Skip(i * size).Take(size);
            }
        }

        public static byte[] Compress(byte[] data)
        {
            byte[] compressArray = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                    compressArray = memoryStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }

            return compressArray;
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            try
            {
                using (MemoryStream decompressedStream = new MemoryStream())
                using (MemoryStream compressStream = new MemoryStream(data))
                using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                    decompressedArray = decompressedStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                // do something !
            }

            return decompressedArray;
        }

    }
}
