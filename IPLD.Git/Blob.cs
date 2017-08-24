using System;
using System.Collections.Generic;
using System.Text;
using IPLD.ContentIdentifier;
using IPLD.Format;
using Multiformats.Codec;
using Multiformats.Hash;
using Org.BouncyCastle.Crypto.Generators;

namespace IPLD.Git
{
    public class Blob : INode
    {
        private readonly byte[] _bytes;
        private readonly Lazy<Cid> _cid;

        public Blob(byte[] bytes)
        {
            _bytes = bytes;
            _cid = new Lazy<Cid>(() => new Prefix(1, MulticodecCode.Git, HashType.SHA1, -1).Sum(_bytes));
        }

        public static implicit operator Blob(byte[] bytes) => new Blob(bytes);
        public static implicit operator byte[](Blob blob) => blob._bytes;

        public Cid Cid() => _cid.Value;

        public byte[] RawData() => _bytes;

        public (object, string[]) Resolve(string[] path) => (null, Array.Empty<string>());

        public (Link, string[]) ResolveLink(string[] path) => (null, Array.Empty<string>());

        public string[] GetTree(string path, int depth) => Array.Empty<string>();

        public INode Copy() => new Blob(_bytes.Skip(0));

        public Link[] Links() => Array.Empty<Link>();

        public NodeStat Stat() => new NodeStat();

        public ulong Size() => (ulong)_bytes.Length;

        public override string ToString() => "[git blob]";

        public byte[] GitSha() => Cid().ToSha();
    }
}
