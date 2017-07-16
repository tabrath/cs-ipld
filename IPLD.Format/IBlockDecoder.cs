using System;
using IPLD.BlockFormat;
using Multiformats.Codec;

namespace IPLD.Format
{
    public interface IBlockDecoder
    {
        void Register(MulticodecCode codec, Func<IBlock, INode> decoder);
        INode Decode(IBlock block);
    }
}
