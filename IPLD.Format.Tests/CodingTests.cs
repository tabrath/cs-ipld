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
            // @todo: we've just opened up the constructor in Prefix so we can create
            // a prefix without parsing a byte array because this does not seem to work.
            // It's currently in the deployment process.  -tabrath 17/7/17

            var prefix = new Mock<Prefix>();
            prefix.Setup(p => p.Codec).Returns(MulticodecCode.Raw);
            prefix.Setup(p => p.Version).Returns(1);
            prefix.Setup(p => p.MultihashType).Returns(HashType.ID);
            prefix.Setup(p => p.MultihashLength).Returns(0);

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
            node.Setup(n => n.Cid).Returns(() => prefix.Object.Sum(null));

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
            var prefix = new Mock<Prefix>();
            prefix.Setup(i => i.Version).Returns(1);
            prefix.Setup(i => i.Codec).Returns(MulticodecCode.Raw);
            prefix.Setup(i => i.MultihashType).Returns(HashType.ID);
            prefix.Setup(i => i.MultihashLength).Returns(0);
            var id = prefix.Object.Sum(null);

            var block = new BasicBlock(null, id);
            var node = BlockDecoder.Default.Decode(block);
            
            Assert.NotNull(node);
            Assert.Equal(id, node.Cid);
        }
    }
}
