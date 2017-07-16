using System;
using IPLD.BlockFormat;
using IPLD.ContentIdentifier;
using Moq;
using Multiformats.Codec;
using Multiformats.Hash;
using Xunit;

namespace IPLD.Format.Tests
{
    public class CodingTests
    {
        public CodingTests()
        {
            var prefix = new Prefix(1, MulticodecCode.Raw, HashType.ID, 0);

            var node = new Mock<INode>();
            node.Setup(n => n.Links).Returns(Array.Empty<Link>());
            node.Setup(n => n.Copy()).Returns(node.Object);
            node.Setup(n => n.Resolve(It.IsAny<string[]>())).Returns((null, Array.Empty<string>()));
            node.Setup(n => n.Tree(It.IsAny<string>(), It.IsAny<int>())).Returns(Array.Empty<string>());
            node.Setup(n => n.ResolveLink(It.IsAny<string[]>())).Returns((null, Array.Empty<string>()));
            node.Setup(n => n.ToString()).Returns("[]");
            node.Setup(n => n.RawData).Returns(Array.Empty<byte>());
            node.Setup(n => n.Size()).Returns(0);
            node.Setup(n => n.Stat()).Returns(new NodeStat());
            node.Setup(n => n.Cid).Returns(() => prefix.Sum(Array.Empty<byte>()));

            BlockDecoder.Default.Register(MulticodecCode.Raw, b =>
            {
                var n = node.Object;
                if (!b.RawData.Equals(Array.Empty<byte>()) || !b.Cid.Equals(n.Cid))
                    throw new NotSupportedException("can only decode empty blocks");

                return n;
            });
        }

        [Fact]
        public void Decode_GivenBlockWithCid_ReturnsCorrectNode()
        {
            var id = new Prefix(1, MulticodecCode.Raw, HashType.ID, 0).Sum(Array.Empty<byte>());

            var block = new BasicBlock(null, id);
            var node = BlockDecoder.Default.Decode(block);
            
            Assert.NotNull(node);
            Assert.Equal(id, node.Cid);
        }
    }
}
