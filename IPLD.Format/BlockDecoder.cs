using System;
using System.Collections.Concurrent;
using IPLD.BlockFormat;
using Multiformats.Codec;

namespace IPLD.Format
{
    public class BlockDecoder : IBlockDecoder
    {
        public static readonly IBlockDecoder Default = new BlockDecoder();

        private readonly ConcurrentDictionary<MulticodecCode, Func<IBlock, INode>> _decoders;

        private BlockDecoder()
        {
            _decoders = new ConcurrentDictionary<MulticodecCode, Func<IBlock, INode>>();
        }

        public void Register(MulticodecCode codec, Func<IBlock, INode> decoder)
        {
            _decoders.AddOrUpdate(codec, decoder, (c, existing) => decoder);
        }

        public INode Decode(IBlock block)
        {
            if (block is INode)
                return (INode) block;

            var type = block.Cid.Type;
            if (!_decoders.TryGetValue(type, out Func<IBlock, INode> decoder))
                throw new NotSupportedException($"Unrecognized object type: {type}");

            return decoder(block);
        }
    }
}