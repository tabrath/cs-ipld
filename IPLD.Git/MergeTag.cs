using IPLD.ContentIdentifier;

namespace IPLD.Git
{
    public class MergeTag
    {
        public Cid Object { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public PersonInfo Tagger { get; set; }
        public string Text { get; set; }
    }
}