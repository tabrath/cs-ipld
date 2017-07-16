using IPLD.ContentIdentifier;

namespace IPLD.BlockFormat
{
    public interface IBlock
    {
        byte[] RawData { get; }
        Cid Cid { get; }
    }
}