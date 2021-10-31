namespace StrongFilePath
{
    public readonly partial struct FilePath
    {
        public enum KindOnDevice : byte
        {
            None,
            File,
            Directory,
        }
    }
}