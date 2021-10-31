using System;
using System.Buffers;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HeaplessUtility;

namespace StrongFilePath
{
    [DebuggerDisplay("{FullFilePath,nq}")]
    public readonly partial struct FilePath
    {
        public static readonly char[] DirectorySeparators = {
            '\\', '/',
        };
        
        public const char ExtensionSeparator = '.';

        internal static readonly Regex DirectoryRootRegex = new(@"^[a-zA-Z]+:[\/\\]", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        internal static readonly Regex DirectoryRelativeRegex = new(@"^(?:\.*|[^<>:""|?*]*)[\/\\]", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        internal static readonly Regex PermissivePathRegex = new(@"^\\*(?:\?\\)?[^<>""|?*\\\/]+(?:[\\\/]+[^<>"":|?*\\\/]+)*$", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        
        private readonly FilePathInfo _filePathInfo;
        private readonly string? _fullPath;
        
        /// <summary>
        ///     Initializes a new <see cref="FilePath"/> with the specified <paramref name="fullPath"/>.
        /// </summary>
        /// <remarks>
        ///     The <paramref name="fullPath"/> is not escaped, and the file path may be invalid.
        /// </remarks>
        public FilePath(string fullPath)
        {
            _fullPath = fullPath;
            _filePathInfo = GetFilePathInfo(fullPath);
        }

        /// <summary>
        ///     Whether the <see cref="FilePath"/> is default or empty.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fullPath is null || 0 >= _fullPath.Length;
        }

        /// <summary>
        ///     The string representing the <see cref="FilePath"/>.
        /// </summary>
        public string FullFilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fullPath ?? String.Empty;
        }

        /// <summary>
        ///     The features of the <see cref="FullFilePath"/>.
        /// </summary>
        public PathFlags Flags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filePathInfo.Flags;
        }

        /// <summary>
        ///     Whether any directory portion is present.
        /// </summary>
        /// <example>
        ///     <see langword="true"/> for:
        ///     <list type="">
        ///         <item>"\\?\C:\dir\to\file.txt"</item>
        ///         <item>"\\?\C:\dir\"</item>
        ///         <item>".\file.txt"</item>
        ///         <item>".\"</item>
        ///         <item>"C:\"</item>
        ///         <item>"ftp:\"</item>
        ///      </list>
        ///     <see langword="false"/> for:
        ///     <list type="">
        ///         <item>"file.txt"</item>
        ///         <item>".gitignore"</item>
        ///         <item>&lt;empty&gt;</item>
        ///      </list>
        /// </example>
        public bool HasDirectory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_filePathInfo.Flags & PathFlags.Directory) != PathFlags.None;
        }

        /// <summary>
        ///     Whether any file name portion is present
        /// </summary>
        /// <example>
        ///     <see langword="true"/> for:
        ///     <list type="">
        ///         <item>"\\?\C:\dir\to\file.txt"</item>
        ///         <item>"\\?\C:\dir"</item>
        ///         <item>".\file"</item>
        ///         <item>"file.txt"</item>
        ///         <item>"file"</item>
        ///         <item>".gitignore"</item>
        ///      </list>
        ///     <see langword="false"/> for:
        ///     <list type="">
        ///         <item>"\\?\C:\dir\"</item>
        ///         <item>".\"</item>
        ///         <item>&lt;empty&gt;</item>
        ///      </list>
        /// </example>
        public bool HasFileName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_filePathInfo.Flags & PathFlags.FileNameWithExtension) != PathFlags.None;
        }

        /// <summary>
        ///     Whether the file name portion has a file extension
        /// </summary>
        /// <example>
        ///     Always <see langword="false"/> if not <see cref="HasFileName"/>.<br/> 
        ///     <see langword="true"/> for:
        ///     <list type="">
        ///         <item>"file.txt"</item>
        ///         <item>".gitignore"</item>
        ///      </list>
        ///     <see langword="false"/> for:
        ///     <list type="">
        ///         <item>"file"</item>
        ///      </list>
        /// </example>
        public bool HasFileExtension
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_filePathInfo.Flags & PathFlags.FileExtension) != PathFlags.None;
        }
        
        /// <summary>
        ///     The directory portion.
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>"\\?\C:\dir\to\file.txt" -> "\\?\C:\dir\"</item>
        ///         <item>".\file.txt" -> ".\"</item>
        ///         <item>"ftp:\file" -> "ftp:\"</item>
        ///         <item>"file.txt" -> &lt;empty&gt;</item>
        ///     </list>
        /// </example>
        public ReadOnlySpan<char> DirectoryPath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FullFilePath.AsSpan(0, _filePathInfo.LastDirectorySeparator);
        }
        
        /// <summary>
        ///     The file name portion.
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>"\\?\C:\dir\to\file.txt" -> "file.txt"</item>
        ///         <item>".\file" -> "file"</item>
        ///         <item>".gitignore" -> ".gitignore"</item>
        ///         <item>"\?\C:\dir\" -> &lt;empty&gt;</item>
        ///     </list>
        /// </example>
        public ReadOnlySpan<char> FileName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FullFilePath.AsSpan(_filePathInfo.LastDirectorySeparator);
        }
        
        /// <summary>
        ///     The file name portion without the file extension
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>"your.file.txt" -> "your.file"</item>
        ///         <item>"file" -> "file"</item>
        ///         <item>".gitignore" -> &lt;empty&gt;</item>
        ///     </list>
        /// </example>
        public ReadOnlySpan<char> FileNameWithoutExtension
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ReadOnlySpan<char> filePathWithoutExtension = HasFileExtension ? FullFilePath.AsSpan(0, _filePathInfo.FileExtensionSeparator) : FullFilePath.AsSpan();
                return filePathWithoutExtension.Slice(_filePathInfo.LastDirectorySeparator);
            }
        }
        
        /// <summary>
        ///     The file extension of the file name portion
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>"your.file.txt" -> ".txt"</item>
        ///         <item>".gitignore" -> ".gitignore"</item>
        ///         <item>"file" -> &lt;empty&gt;</item>
        ///     </list>
        /// </example>
        public ReadOnlySpan<char> Extension
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => HasFileExtension ? FullFilePath.AsSpan(_filePathInfo.FileExtensionSeparator) : ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        ///     The file extension excluding the separating dot of the file name portion
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>"your.file.txt" -> "txt"</item>
        ///         <item>".gitignore" -> "gitignore"</item>
        ///         <item>"file" -> &lt;empty&gt;</item>
        ///     </list>
        /// </example>
        public ReadOnlySpan<char> ExtensionWithoutDot
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => HasFileExtension ? FullFilePath.AsSpan(_filePathInfo.FileExtensionSeparator + 1) : ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        ///     Iterates over all portions of the <see cref="FilePath"/>.
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>".\file.txt" -> [".\", "file.txt"]</item>
        ///         <item>"file" -> ["file"]</item>
        ///         <item>"C:\\dir/to\file.txt" -> ["C:\\", "dir/", "to\", "file.txt"]</item>
        ///         <item>"\\?\ftp:\dir\to\file.txt" -> ["\\", "?\", "ftp:\", "dir\", "to\", "file.txt"]</item>
        ///     </list>
        /// </example>
        public PathSegmentEnumerator GetEnumerator()
        {
            return new PathSegmentEnumerator(FullFilePath);
        }

        public override string ToString()
        {
            return FullFilePath;
        }

        /// <summary>
        ///     Normalizes the <see cref="FullFilePath"/>.
        /// </summary>
        /// <returns>A new <see cref="FilePath"/> from <see cref="Path.GetFullPath(string)"/></returns>
        /// <exception cref="ArgumentException"><see cref="IsEmpty"/> or !<see cref="IsValid()"/></exception>
        public FilePath Normalize()
        {
            return Path.GetFullPath(FullFilePath).ToFilePath();
        }

        /// <summary>
        ///     Indicates whether <see cref="FullFilePath"/> could possibly represent a file path.
        /// </summary>
        /// <remarks>
        ///     False negative matches are not possible.
        ///     False positive matches are possible.
        /// 
        /// </remarks>
        public bool IsValid()
        {
            if (String.IsNullOrEmpty(_fullPath))
            {
                return false;
            }

            return PermissivePathRegex.IsMatch(_fullPath);
        }

        /// <summary>
        ///     Indicates whether <see cref="FullFilePath"/> could possibly represent file name.
        /// </summary>
        /// <returns><see langword="false"/> if <see cref="FullFilePath"/> is empty, whitespace or contains any illegal character (&lt;&gt;:"|?*/\), otherwise <see langword="true"/>.</returns>
        public bool IsValidFileName()
        {
            if (FullFilePath.IsNullOrEmpty())
            {
                return false;
            }

            ReadOnlySpan<char> fileName = FullFilePath.AsSpan();
            int index = 0;
            while (true)
            {
                char ch = fileName[index++];

                if (!Char.IsWhiteSpace(ch))
                {
                    break;
                }
                if (index == fileName.Length)
                {
                    return false;
                }
            }
            while (index < fileName.Length)
            {
                char ch = fileName[index++];

                switch (ch)
                {
                    case '<':
                    case '>':
                    case ':':
                    case '"':
                    case '|':
                    case '?':
                    case '*':
                    case '\\':
                    case '/':
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Returns a new <see cref="FilePath"/> replacing the file extension with the specified string.
        /// </summary>
        /// <remarks>
        ///     <list type="">
        ///         <item>If the <paramref name="newExtension"/> does not start with a dot ('.'), inserts a dot before the extension.</item>
        ///         <item>If the <see cref="DirectoryPath"/> has no extension, appends the <paramref name="newExtension"/>.</item>
        ///     </list>
        /// </remarks>
        public FilePath ReplaceExtension(ReadOnlySpan<char> newExtension)
        {
            int length = FullFilePath.Length + newExtension.Length - ExtensionWithoutDot.Length;
            Builder builder = length < 2048 ? new Builder(stackalloc char[length]) : new Builder(length);
            builder.Append(DirectoryPath);
            builder.Combine(FileNameWithoutExtension);
            builder.AppendExtension(newExtension);
            return builder.ToFilePath();
        }

        public bool ExtensionEquals(ReadOnlySpan<char> extension)
        {
            if (!HasFileExtension && extension.IsEmpty)
            {
                return true;
            }
            if (!HasFileExtension || extension.IsEmpty)
            {
                return false;
            }
            return ExtensionWithoutDot.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase);
        }

        public FileInfo GetFileInfo()
        {
            return new FileInfo(FullFilePath);
        }

        public DirectoryInfo GetDirectoryInfo()
        {
            return new DirectoryInfo(FullFilePath);
        }

        public bool Exists()
        {
            return OnDevice() != KindOnDevice.None;
        }

        /// <summary>
        ///     Queries the file system about if the <see cref="FilePath"/> exists.
        ///     
        /// </summary>
        /// <returns>If <see cref="HasFileName"/> returns whether the file is a directory or a file, otherwise; returns if the directory exists.</returns>
        public KindOnDevice OnDevice()
        {
            if (!HasFileName)
            {
                return Directory.Exists(FullFilePath) ? KindOnDevice.Directory : KindOnDevice.None;
            }

            if (File.Exists(FullFilePath))
            {
                return KindOnDevice.File;
            }

            if (Directory.Exists(FullFilePath))
            {
                return KindOnDevice.Directory;
            }

            return KindOnDevice.None;
        }

        /// <summary>
        ///     Provides a read-only <see cref="FileStream"/> for the file. Opening an existing file.
        /// </summary>
        /// <param name="shared">Whether the read is shared globally or read is locked.</param>
        public FileStream OpenRead(bool shared = true)
        {
            return shared ? new FileStream(FullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read) : new FileStream(FullFilePath, FileMode.Open, FileAccess.Read);
        }


        /// <summary>
        ///     Provides a write <see cref="FileStream"/> for the file. Opening an existing file.
        /// </summary>
        /// <param name="shared">Whether the read and write is shared globally or write is locked.</param>
        public FileStream OpenWrite(bool shared = true)
        {
            return shared ? new FileStream(FullFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite) : new FileStream(FullFilePath, FileMode.Open, FileAccess.Write);
        }

        /// <summary>
        ///     Provides a write <see cref="FileStream"/> for the file. Creating a new file or overwriting a existing file. 
        /// </summary>
        /// <param name="shared">Whether the read and write is shared globally or write is locked.</param>
        /// <returns></returns>
        public FileStream Create(bool shared = true)
        {
            return shared ? new FileStream(FullFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite) : new FileStream(FullFilePath, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        ///     Instantiates a new <see cref="FilePath.Builder"/> based on <see cref="FullFilePath"/>.
        /// </summary>
        public Builder ToBuilder() => ToBuilder(16);

        /// <summary>
        ///     Instantiates a new <see cref="FilePath.Builder"/> based on <see cref="FullFilePath"/>.
        /// </summary>
        /// <param name="additionalCapacity">The amount of characters beyond <see cref="FullFilePath"/> the <see cref="FilePath.Builder"/> can initially hold.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="additionalCapacity"/> lt; 0</exception>
        public Builder ToBuilder(int additionalCapacity)
        {
            if (additionalCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(additionalCapacity));
            }
            Builder builder = new(FullFilePath.Length + Math.Max(16, additionalCapacity));
            builder.Append(FullFilePath.AsSpan());
            return builder;
        }

        /// <summary>
        ///     Combines the <see cref="FilePath"/> with additional path segments using a back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public FilePath CombineWith(ReadOnlySpan<char> pathSegment)
        {
            return Combine(FullFilePath.AsSpan(), pathSegment);
        }

        /// <summary>
        ///     Combines the <see cref="FilePath"/> with additional path segments using a back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public FilePath CombineWith(ReadOnlySpan<char> pathSegment1, ReadOnlySpan<char> pathSegment2)
        {
            return Combine(FullFilePath.AsSpan(), pathSegment1, pathSegment2);
        }

        /// <summary>
        ///     Combines the <see cref="FilePath"/> with additional path segments using a back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public FilePath CombineWith(ReadOnlySpan<char> pathSegment1, ReadOnlySpan<char> pathSegment2, ReadOnlySpan<char> pathSegment3)
        {
            return Combine(FullFilePath.AsSpan(), pathSegment1, pathSegment2, pathSegment3);
        }

        /// <summary>
        ///     Combines path segments delimiting each segment using a single back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public static FilePath Combine(ReadOnlySpan<char> pathSegment1, ReadOnlySpan<char> pathSegment2)
        {
            int len = pathSegment1.Length + pathSegment2.Length + 2;
            Builder builder = len < 2048 ? new(stackalloc char[len]) : new(len);
            builder.Combine(pathSegment1);
            builder.Combine(pathSegment2);
            return builder.ToFilePath();
        }

        /// <summary>
        ///     Combines path segments delimiting each segment using a back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public static FilePath Combine(ReadOnlySpan<char> pathSegment1, ReadOnlySpan<char> pathSegment2, ReadOnlySpan<char> pathSegment3)
        {
            int len = pathSegment1.Length + pathSegment2.Length + pathSegment3.Length + 3;
            Builder builder = len < 2048 ? new(stackalloc char[len]) : new(len);
            builder.Combine(pathSegment1);
            builder.Combine(pathSegment2);
            builder.Combine(pathSegment3);
            return builder.ToFilePath();
        }

        /// <summary>
        ///     Combines path segments delimiting each segment using a back-slash ('\').
        /// </summary>
        /// <returns>The resulting <see cref="FilePath"/>.</returns>
        public static FilePath Combine(ReadOnlySpan<char> pathSegment1, ReadOnlySpan<char> pathSegment2, ReadOnlySpan<char> pathSegment3, ReadOnlySpan<char> pathSegment4)
        {
            int len = pathSegment1.Length + pathSegment2.Length + pathSegment3.Length + pathSegment4.Length + 4;
            Builder builder = len < 2048 ? new(stackalloc char[len]) : new(len);
            builder.Combine(pathSegment1);
            builder.Combine(pathSegment2);
            builder.Combine(pathSegment3);
            builder.Combine(pathSegment4);
            return builder.ToFilePath();
        }

        /// <summary>
        ///     Combines the directory, file name, and extension portions into a <see cref="FilePath"/>.  
        /// </summary>
        /// <example>
        ///     <list type="">
        ///         <item>["C:\dir\to", "file", "txt"] -> "C:\dir\to\file.txt"</item>
        ///         <item>[".\", "\file", ".txt"] -> ".\file.txt"</item>
        ///         <item>[".\\", "\file", ".txt"] -> ".\\file.txt"</item>
        ///     </list>
        /// </example>
        public static FilePath CombineDirectoryNameExtension(ReadOnlySpan<char> directoryPath, ReadOnlySpan<char> fileName, ReadOnlySpan<char> extension)
        {
            int len = directoryPath.Length + fileName.Length + extension.Length + 2;
            Builder builder = len < 2048 ? new(stackalloc char[len]) : new(len);
            builder.Append(directoryPath);
            builder.Combine(fileName);
            builder.AppendExtension(extension);
            return builder.ToFilePath();
        }

        /// <summary>
        ///     Queries all entries in the specified environment variable for the <paramref name="fileName"/>.   
        /// </summary>
        /// <param name="envVariableName">The name of the environment variable. e.g. PATH</param>
        /// <param name="fileName">The name of the file in the entry.</param>
        /// <returns>The first <see cref="FilePath"/> of the file with the specified name within the entries of the environment variable.</returns>
        public static FilePath ResolveFileNameFromEnv(string envVariableName, ReadOnlySpan<char> fileName)
        {
            string? environmentVariableContent = Environment.GetEnvironmentVariable(envVariableName);
            if (environmentVariableContent.IsNullOrEmpty())
            {
                return default;
            }
            foreach (ReadOnlySpan<char> pathEntry in environmentVariableContent.AsSpan().Split(';', SplitOptions.RemoveEmptyEntries))
            {
                FilePath filePath = FilePath.Combine(pathEntry.Slice(0, pathEntry.Length - 1), fileName);
                if (filePath.Exists())
                {
                    return filePath;
                }
            }
            return default;
        }
        
        private static FilePathInfo GetFilePathInfo(string fullFilePath)
        {
            ReadOnlySpan<char> filePathSpan = fullFilePath.AsSpan();
            int lastDirectorySeparator = filePathSpan.LastIndexOfAny(DirectorySeparators) + 1;
            ReadOnlySpan<char> fileName = filePathSpan.Slice(lastDirectorySeparator);
            int fileExtensionSeparator = lastDirectorySeparator + fileName.LastIndexOf(ExtensionSeparator);
            
            PathFlags flags = PathFlags.None;
                
            // C:\
            if (DirectoryRootRegex.IsMatch(fullFilePath))
            {
                flags |= PathFlags.DirectoryRoot;
            }
            
            // .\
            if (DirectoryRelativeRegex.IsMatch(fullFilePath))
            {
                flags |= PathFlags.DirectoryRelative;
            }
            
            // .gitignore
            if (lastDirectorySeparator != filePathSpan.Length)
            {
                flags |= PathFlags.FileName;
            }
            
            // README
            if (fileExtensionSeparator >= lastDirectorySeparator)
            {
                flags |= PathFlags.FileExtension;
            }
            
            return new FilePathInfo(lastDirectorySeparator, fileExtensionSeparator, flags);
        }

        public static explicit operator FilePath(string self) => new(self);

        public static explicit operator FilePath(ReadOnlySpan<char> self) => new(self.ToString());
            
        public static implicit operator string(in FilePath self) => self.FullFilePath;

        public static implicit operator ReadOnlySpan<char>(in FilePath self) => self.FullFilePath.AsSpan();

        /// <summary>
        ///     The <see cref="FilePath"/> of <see cref="String.Empty"/>.
        /// </summary>
        public static FilePath Empty => new(String.Empty);

        private readonly struct FilePathInfo
        {
            public readonly int LastDirectorySeparator;

            public readonly int FileExtensionSeparator;

            public readonly PathFlags Flags;

            public FilePathInfo(int lastDirectorySeparator, int fileExtensionSeparator, PathFlags flags)
            {
                LastDirectorySeparator = lastDirectorySeparator;
                FileExtensionSeparator = fileExtensionSeparator;
                Flags = flags;
            }
        }
    }
}
