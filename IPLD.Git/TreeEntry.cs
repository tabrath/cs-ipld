using System.IO;
using System.Text;
using IPLD.ContentIdentifier;

namespace IPLD.Git
{
    public class TreeEntry
    {
        internal string name;

        public string Mode { get; set; }
        public Cid Hash { get; set; }

        public int WriteTo(Stream stream)
        {
            var n = stream.Write($"{Mode} {name}\x00");
            if (n <= 0)
                return 0;

            var nn = stream.Write(Hash.ToSha());
            if (nn <= 0)
                return n;

            return n + nn;
        }

        public static (TreeEntry entry, bool eof) Read(Stream stream)
        {
            try
            {
                var data = stream.ReadString(' ');
                if (string.IsNullOrEmpty(data))
                    return (null, false);

                data = data.Substring(0, data.Length - 1);

                var name = stream.ReadString(0);
                if (string.IsNullOrEmpty(name))
                    return (null, false);

                name = name.Substring(0, name.Length - 1);

                var sha = new byte[20];
                if (stream.Read(sha, 0, 20) != 20)
                    return (null, false);

                return (new TreeEntry
                {
                    name = name,
                    Mode = data,
                    Hash = sha.AsShaToCid()
                }, false);
            }
            catch (EndOfStreamException)
            {
                return (null, true);
            }
        }
    }
}