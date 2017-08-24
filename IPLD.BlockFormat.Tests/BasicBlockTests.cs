using System;
using System.Diagnostics;
using System.Text;
using IPLD.ContentIdentifier;
using Multiformats.Hash;
using Xunit;

namespace IPLD.BlockFormat.Tests
{
    public class BasicBlockTests
    {
        [Fact]
        public void Constructor_GivenEmptyByteArray_DoesNotThrow()
        {
            var block = new BasicBlock(Array.Empty<byte>());

            Assert.NotNull(block);
        }

        [Fact]
        public void Constructor_GivenNull_DoesNotThrow()
        {
            var block = new BasicBlock(null);

            Assert.NotNull(block);
        }

        [Fact]
        public void Constructor_GivenData_DoesNotThrow()
        {
            var block = new BasicBlock(Encoding.UTF8.GetBytes("Hello world!"));

            Assert.NotNull(block);
        }

        [Fact]
        public void Constructor_GivenData_HasEqualRawData()
        {
            var data = Encoding.UTF8.GetBytes("some data");
            var block = new BasicBlock(data);

            Assert.Equal(data, block.RawData());
        }

        [Fact]
        public void Constructor_GivenData_HasCorrectMultihash()
        {
            var data = Encoding.UTF8.GetBytes("some other data");
            var block = new BasicBlock(data);
            var hash = Multihash.Sum(HashType.SHA2_256, data);

            Assert.Equal(hash, block.Multihash);
        }

        [Fact]
        public void Constructor_GivenData_HasCorrectCidHash()
        {
            var data = Encoding.UTF8.GetBytes("yet another data");
            var block = new BasicBlock(data);
            var cid = block.Cid();

            Assert.Equal(block.Multihash, cid.Hash);
        }

        [Fact]
        public void Constructor_GivenDataAndCid_HasValidHash()
        {
            var data = Encoding.UTF8.GetBytes("I can't figure out more names .. data");
            var hash = Multihash.Sum(HashType.SHA2_256, data);
            var cid = new Cid(hash);
            var block = new BasicBlock(data, cid);

            Assert.Equal(hash, block.Multihash);
        }

        [Fact]
        public void Constructor_GivenMismatchingDataAndCid_IsNotEqualHash()
        {
            var data = Encoding.UTF8.GetBytes("I can't figure out more names .. data");
            var hash = Multihash.Sum(HashType.SHA2_256, data);
            var cid = new Cid(hash);

            data[5] = (byte)((data[5] + 5) % 256);

#if DEBUG
            Assert.Throws<InvalidHashException>(() => new BasicBlock(data, cid));
#else
            new BasicBlock(data, cid);
#endif
        }
    }
}
