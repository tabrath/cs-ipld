using IPLD.ContentIdentifier;

namespace IPLD.BlockFormat
{
    public interface IBlock
    {
        byte[] RawData();
        Cid Cid();
    }
}