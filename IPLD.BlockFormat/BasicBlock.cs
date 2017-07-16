using System;
using IPLD.ContentIdentifier;
using Multiformats.Hash;

namespace IPLD.BlockFormat
{
    public class BasicBlock : IBlock
    {
        public byte[] RawData { get; }
        public Cid Cid { get; }

        public Multihash Multihash => Cid.Hash;

        public BasicBlock(byte[] data, Cid cid = null)
        {
            RawData = data ?? Array.Empty<byte>();
#if DEBUG
            var chk = cid?.Prefix.Sum(RawData);
            if (!chk?.Equals(cid) ?? false)
                throw new InvalidHashException(chk, cid);
#endif
            Cid = cid ?? new Cid(RawData.Hash());
        }

        public override string ToString() => $"[Block {Cid}]";
    }
}
