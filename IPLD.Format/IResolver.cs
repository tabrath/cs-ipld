namespace IPLD.Format
{
    public interface IResolver
    {
        (object, string[]) Resolve(string[] path);
        string[] Tree(string path, int depth);
    }
}
