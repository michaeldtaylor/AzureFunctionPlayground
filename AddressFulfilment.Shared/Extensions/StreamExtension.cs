using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AddressFulfilment.Shared.Extensions
{
    public static class StreamExtension
    {
        public static string AsString(this Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var streamReader = new StreamReader(stream);
            var result = streamReader.ReadToEnd();

            return result;
        }

        public static byte[] AsBytes(this Stream stream)
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                stream.Rewind();
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return bytes;
        }

        public static string GenerateChecksum(this Stream stream)
        {
            var hash = SHA256.Create().ComputeHash(stream);

            stream.Rewind();

            return Convert.ToBase64String(hash);
        }

        public static void Rewind(this Stream stream)
        {
            stream.Seek(0L, SeekOrigin.Begin);
        }

        public static byte[] Compress(this Stream stream)
        {
            using (var compressedStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressedStream, CompressionMode.Compress))
            {
                stream.CopyTo(compressor);
                compressor.Close();

                return compressedStream.ToArray();
            }
        }
        
        public static byte[] GetBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
    }
}