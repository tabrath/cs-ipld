using System;
using System.Threading.Tasks;
using IPLD.ContentIdentifier;

namespace IPLD.Format
{
    public class Link
    {
        public string Name { get; set; }
        public ulong Size { get; set; }
        public Cid Cid { get; set; }

        public Task<INode> GetNode(Func<Cid, Task<INode>> getter) => getter(Cid);
    }
}