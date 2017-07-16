namespace IPLD.Format
{
    public class NodeStat
    {
        public string Hash { get; set; }
        public int NumLinks { get; set; }
        public int BlockSize { get; set; }
        public int LinksSize { get; set; }
        public int DataSize { get; set; }
        public int CumulativeSize { get; set; }

        public override string ToString() => string.Format(
            "NodeStat{NumLinks: {0}, BlockSize: {1}, LinksSize: {2}, DataSize: {3}, CumulativeSize: {4}}",
            NumLinks, BlockSize, LinksSize, DataSize, CumulativeSize);
    }
}