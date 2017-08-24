namespace IPLD.Format
{
    public interface IResolver
    {
        (object, string[]) Resolve(string[] path);
        string[] GetTree(string path, int depth);
    }
}
