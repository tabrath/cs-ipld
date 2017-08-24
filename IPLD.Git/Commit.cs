using System;
using System.IO;
using System.Linq;
using IPLD.ContentIdentifier;
using IPLD.Format;

namespace IPLD.Git
{
    public class Commit : INode
    {
        public string DataSize { get; set; }
        public Cid GitTree { get; set; }
        public Cid[] Parents { get; set; }
        public string Message { get; set; }
        public PersonInfo Author { get; set; }
        public PersonInfo Committer { get; set; }
        public string Encoding { get; set; }
        public GpgSignature Signature { get; set; }
        public MergeTag[] MergeTag { get; set; }

        public string[] Other { get; set; }

        internal Cid cid;

        public Cid Cid() => cid;

        public byte[] RawData()
        {
            using (var stream = new MemoryStream())
            {
                stream.Write($"commit {DataSize}\x00");
                stream.Write($"tree {GitTree.ToSha().ToHex()}\n");

                foreach (var parent in Parents)
                    stream.Write($"parent {parent.ToSha().ToHex()}\n");

                stream.Write($"author {Author}\n");
                stream.Write($"committer {Committer}\n");

                if (!string.IsNullOrEmpty(Encoding))
                    stream.Write($"encoding {Encoding}\n");

                foreach (var mergeTag in MergeTag)
                {
                    stream.Write($"mergetag object {mergeTag.Object.ToSha().ToHex()}\n");
                    stream.Write($" type {mergeTag.Type}\n");
                    stream.Write($" tag {mergeTag.Tag}\n");
                    stream.Write($" tagger {mergeTag.Tagger}\n");
                    stream.Write(mergeTag.Text);
                }

                if (Signature != null)
                    stream.Write($"gpgsig -----BEGIN PGP SIGNATURE-----\n{Signature.Text} -----END PGP SIGNATURE-----\n");

                foreach (var line in Other)
                    stream.Write($"{line}\n");

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
                case "parents" when (path.Length == 1):
                    return (Parents, Array.Empty<string>());

                case "parents":
                    if (!int.TryParse(path[1], out int i))
                        return (null, Array.Empty<string>());

                    if (i < 0 || i >= Parents.Length)
                        return (null, Array.Empty<string>());

                    return (new Link {Cid = Parents[i]}, path.Skip(2));

                case "author" when (path.Length == 1):
                    return (Author, Array.Empty<string>());

                case "author":
                    return Author.Resolve(path.Skip(1));

                case "committer" when (path.Length == 1):
                    return (Committer, Array.Empty<string>());

                case "commiter":
                    return Committer.Resolve(path.Skip(1));

                case "signature":
                    return (Signature.Text, path.Skip(1));

                case "message":
                    return (Message, path.Skip(1));

                case "tree":
                    return (new Link {Cid = GitTree}, path.Skip(1));

                default:
                    return (null, Array.Empty<string>());
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
            if (depth != -1)
                throw new NotImplementedException("Proper tree not yet implemented");

            return new[] {"tree", "parents", "message", "gpgsig"}
                .Concat(Author.GetTree("author", depth))
                .Concat(Committer.GetTree("committer", depth))
                .Concat(Parents.Select((p, i) => $"parents/{i}"))
                .ToArray();
        }

        public INode Copy()
        {
            throw new System.NotImplementedException();
        }

        public Link[] Links()
        {
            return new[] {new Link {Cid = GitTree}}
                .Concat(Parents.Select(p => new Link {Cid = p}))
                .ToArray();
        }

        public NodeStat Stat() => new NodeStat();

        public ulong Size() => 42;

        public override string ToString() => "[git commit object]";

        public byte[] GitSha() => Cid().ToSha();
    }
}