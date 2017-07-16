namespace IPLD.Format
{
    public static class NodeExtensions
    {
        public static Link MakeLink(INode node)
        {
            var size = node.Size();
            if (size == 0)
                return null;

            return new Link
            {
                Size = size,
                Cid = node.Cid
            };
        }
    }
}