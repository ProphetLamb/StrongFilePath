using System.IO;

namespace StrongFilePath
{
    public static class FileExtensions
    {
        public static FilePath GetFilePath(this FileSystemInfo self)
        {
            return new FilePath(self.FullName);
        }
    }
}
