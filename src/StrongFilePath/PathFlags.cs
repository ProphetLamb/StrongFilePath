using System;

namespace StrongFilePath
{
    [Flags]
    public enum PathFlags : byte
    {
        None = 0,
        
        DirectoryRoot = 1 << 0,
        DirectoryRelative = 1 << 1,
        Directory = DirectoryRoot | DirectoryRelative,
        
        FileName = 1 << 2,
        FileExtension = 1 << 3,
        FileNameWithExtension = FileName | FileExtension,
    }
}