using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace dogs
{
    public interface IFileProxy
    {
        string ReadAllText(string path);
    }

    [ExcludeFromCodeCoverage]
    public class FileProxy : IFileProxy
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
