using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IPLD.BlockFormat;
using IPLD.ContentIdentifier;
using IPLD.Format;
using Org.BouncyCastle.Crypto;

namespace IPLD.Git
{
    public class Tree : INode
    {
        internal Dictionary<string, TreeEntry> entries;
        internal int size;
        internal string[] order;
        internal Cid cid;

        public byte[] GitSha() => cid.ToSha();

        public Cid Cid() => cid;

        public INode Copy()
        {
            var copy = new Tree
            {
                entries = new Dictionary<string, TreeEntry>(),
                cid = cid,
                size = size,
                order = order
            };

            // @todo: Really? What?
            foreach (var entry in entries)
            {
                copy.entries[entry.Key] = entry.Value;
            }

            return copy;
        }

        public NodeStat Stat() => NodeStat.Empty;

        public ulong Size() => 13;

        public string[] GetTree(string path, int depth)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!entries.TryGetValue(path, out TreeEntry entry))
                    return Array.Empty<string>();

                return new[] {"mode", "type", "hash"};
            }

            if (depth == 0)
                return Array.Empty<string>();

            if (depth == 1)
                return order;

            return entries.Keys.SelectMany(k => new[] {k, k + "/mode", k + "/type", k + "/hash"}).ToArray();
        }

        public Link[] Links() => entries.Values.Select(v => new Link { Cid = v.Hash }).ToArray();

        public byte[] RawData()
        {
            using (var stream = new MemoryStream())
            {
                stream.Write($"tree {size}\x00");

                foreach (var s in order)
                {
                    entries[s].WriteTo(stream);
                }

                return stream.ToArray();
            }
        }

        public (object, string[]) Resolve(string[] path)
        {
            if (!entries.TryGetValue(path[0], out TreeEntry entry))
                throw new KeyNotFoundException();

            if (path.Length == 1)
                return (entry, Array.Empty<string>());

            switch (path[1])
            {
                case "hash":
                    return (new Link {Cid = entry.Hash}, path.Skip(1));
                case "mode":
                    return (entry.Mode, path.Skip(1));
                default:
                    return (null, Array.Empty<string>());
            }
        }

        public (Link, string[]) ResolveLink(string[] path)
        {
            var (output, rest) = Resolve(path);

            var lnk = output as Link;
            return lnk == null ? (null, Array.Empty<string>()) : (lnk, rest);
        }


        public override string ToString() => "[git tree object]";
    }
}
