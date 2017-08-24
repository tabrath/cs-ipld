using System;
using IPLD.ContentIdentifier;
using Multiformats.Hash;

namespace IPLD.BlockFormat
{
    public class BasicBlock : IBlock
    {
        private readonly byte[] _data;
        private readonly Lazy<Cid> _cid;

        public Multihash Multihash => _cid.Value.Hash;

        public BasicBlock(byte[] data, Cid cid = null)
        {
            _data = data ?? Array.Empty<byte>();
#if DEBUG
            var chk = cid?.Prefix.Sum(_data);
            if (!chk?.Equals(cid) ?? false)
                throw new InvalidHashException(chk, cid);
#endif
            _cid = new Lazy<Cid>(() => cid ?? new Cid(_data.Hash()));
        }

        public Cid Cid() => _cid.Value;

        public byte[] RawData() => _data;

        public override string ToString() => $"[Block {_cid.Value}]";
    }
}
