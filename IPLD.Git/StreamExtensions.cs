using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IPLD.Git
{
    internal static class StreamExtensions
    {
        public static int Write(this Stream stream, byte[] buffer, int offset = 0)
        {
            var start = stream.Position;
            stream.Write(buffer, offset, buffer.Length - offset);
            return (int)(stream.Position - start);
        }

        public static int Write(this Stream stream, string value) => stream.Write(Encoding.UTF8.GetBytes(value));

        public static string ReadString(this Stream stream, char delim) => ReadString(stream, (byte) delim);

        public static string ReadString(this Stream stream, char[] delims) => ReadString(stream, delims.Select(d => (byte)d).ToArray());

        public static string ReadString(this Stream stream, byte delim) => ReadString(stream, new[] {delim});

        public static string ReadString(this Stream stream, byte[] delims)
        {
            var bytes = new List<byte>();
            int input;

            while ((input = stream.ReadByte()) != -1)
            {
                var b = (byte) input;
                bytes.Add(b);

                if (delims.Contains(b))
                    break;
            }

            return bytes.Count > 0 ? Encoding.UTF8.GetString(bytes.ToArray()) : string.Empty;
        }

        private static readonly char[] Newlines = {'\n', '\r'};

        public static (string line, bool eof) ReadLine(this Stream stream)
        {
            try
            {
                var line = ReadString(stream, Newlines);

                return (line.TrimEnd(Newlines), false);
            }
            catch (EndOfStreamException)
            {
                return (String.Empty, true);
            }
        }

        public static string ReadAll(this Stream stream)
        {
            int count = 0;
            int bytesRead = 0;
            var buffer = new byte[4096];
            var sb = new StringBuilder();
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                count += bytesRead;
            }
            return sb.ToString();
        }

        public static int Copy(this Stream source, Stream target)
        {
            int count = 0;
            int bytesRead = 0;
            var buffer = new byte[4096];
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                target.Write(buffer, 0, bytesRead);
                count += bytesRead;
            }
            return count;
        }
    }
}