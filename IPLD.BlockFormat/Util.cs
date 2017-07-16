using System;
using System.Collections.Generic;
using System.Text;
using Multiformats.Hash;

namespace IPLD.BlockFormat
{
    // @todo: this does not belong here
    internal static class Util
    {
        public static HashType DefaultIpfsHash = HashType.SHA2_256;

        public static Multihash Hash(this byte[] data) => Multihash.Sum(DefaultIpfsHash, data);
    }
}
