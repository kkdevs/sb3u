using System.IO;

namespace Jolt.IO
{
    /// <summary>
    /// Defaultimplementation of <see cref="IFile"/> which delegates directly to the methods in <c>System.IO.File</c>.
    /// </summary>
    public class FileProxy : IFile
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public StreamReader OpenText(string path)
        {
            return File.OpenText(path);
        }
    }
}