using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPLD.ContentIdentifier;
using Multiformats.Codec;
using Multiformats.Hash;

namespace IPLD.Git
{
    internal static class Extensions
    {
        public static byte[] ToSha(this Cid cid)
        {
            var hash = cid.Hash.ToBytes();
            return hash.Skip(hash.Length - 20).ToArray();
        }

        public static Cid AsShaToCid(this byte[] sha)
        {
            var hash = Multihash.Encode(sha, HashType.SHA1);
            return new Cid(MulticodecCode.Git, hash); // @todo: is it Git or GitRaw?
        }
    }
}
