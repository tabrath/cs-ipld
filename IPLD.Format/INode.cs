using IPLD.BlockFormat;

namespace IPLD.Format
{
    public interface INode : IBlock, IResolver
    {
        (Link, string[]) ResolveLink(string[] path);
        INode Copy();
        Link[] Links();
        NodeStat Stat();
        ulong Size();
    }
}