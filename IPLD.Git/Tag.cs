using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IPLD.ContentIdentifier;
using IPLD.Format;
using Multiformats.Base;

namespace IPLD.Git
{
    public class Tag : INode
    {
        internal string dataSize;
        internal Cid cid;

        public Cid Object { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public PersonInfo Tagger { get; set; }
        public string Message { get; set; }

        public Link[] Links() => new[] {new Link {Cid = Object}};

        public Cid Cid() => cid;

        public byte[] RawData()
        {
            using (var stream = new MemoryStream())
            {
                stream.Write($"tag {dataSize}\x00");
                stream.Write($"object {Multibase.EncodeRaw(Multibase.Base16, Object.ToSha())}\n");
                stream.Write($"type {Type}\n");
                stream.Write($"tag {Name}\n");

                if (Tagger != null)
                    stream.Write($"tagger {Tagger}\n");

                if (!string.IsNullOrEmpty(Message))
                    stream.Write($"\n{Message}");

                return stream.ToArray();
            }
        }

        public (object, string[]) Resolve(string[] path)
        {
            if (path?.Length == 0)
                return (null, Array.Empty<string>());

            switch (path[0])
            {
                case "object":
                    return (new Link {Cid = Object}, path.Skip(1));
                case "type":
                    return (Type, path.Skip(1));
                case "tagger":
                    return path.Length == 1 ? (Tagger, Array.Empty<string>()) : Tagger.Resolve(path.Skip(1));
                case "message":
                    return (Message, path.Skip(1));
                case "tag":
                    return (Name, path.Skip(1));
                default:
                    return (null, path.Skip(1));
            }
        }

        public (Link, string[]) ResolveLink(string[] path)
        {
            var (output, rest) = Resolve(path);

            var lnk = output as Link;
            if (lnk == null)
                return (null, Array.Empty<string>());

            return (lnk, rest);
        }

        public string[] GetTree(string path, int depth)
        {
            if (!string.IsNullOrEmpty(path))
                return path == "tagger" ? new[] {"name", "email", "date"} : Array.Empty<string>();

            if (depth == 0)
                return Array.Empty<string>();

            return new[] {"object", "type", "tag", "message"}.Concat(Tagger.GetTree("tagger", depth)).ToArray();
        }

        public INode Copy()
        {
            return new Tag
            {
                Object = Object,
                Type = Type,
                Name = Name,
                Tagger = Tagger,
                Message = Message,
                dataSize = dataSize,
                cid = cid
            };
        }

        public NodeStat Stat() => new NodeStat();

        public ulong Size() => 42;

        public override string ToString() => "[git tag object]";

        public byte[] GitSha() => Cid().ToSha();
    }
}